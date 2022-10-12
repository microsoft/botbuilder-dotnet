##############################################################
# Copy Azure DevOps work items to GitHub issues
##############################################################

# From: https://github.com/joshjohanning/ado_workitems_to_github_issues/blob/main/ado_workitems_to_github_issues.ps1

# Prerequisites:
# 1. Install az devops and github cli
# 2. create a label for EACH work item type that is being migrated (as lower case) 
#      - ie: "user story", "bug", "task", "feature"
# 3. define under what area path you want to migrate
#      - You can modify the WIQL if you want to use a different way to migrate work items, such as [TAG] = "migrate"

# How to run:
# ./AdoWorkitemsToGithubIssues.ps1 -ado_pat "xxx" -ado_org "fuselabs" -ado_project "SDK_v4" -ado_area_path "SDK_v4\Code Analysis" `
#   -gh_pat "xxx" -gh_org "microsoft" -gh_repo "botbuilder-dotnet" -gh_update_assigned_to -gh_assignee "tracyboehrer" -gh_assigned_to_user_suffix "" -gh_add_ado_comments

#
# Things it migrates:
# 1. Title
# 2. Description (or repro steps + system info for a bug)
# 3. State (if the work item is done / closed, it will be closed in GitHub)
# 4. It will try to assign the work item to the correct user in GitHub - based on ADO email (-gh_update_assigned_to and -gh_assigned_to_user_suffix options) - they of course have to be in GitHub already
# 5. Migrate acceptance criteria as part of issue body (if present)
# 6. Adds in the following as a comment to the issue:
#   a. Original work item url 
#   b. Basic details in a collapsed markdown table
#   c. Entire work item as JSON in a collapsed section
# 7. Creates tag "copied-to-github" and a comment on the ADO work item. The tag prevents duplicate copying.
#

#
# Things it won't ever migrate:
# 1. Created date/update dates
# 

[CmdletBinding()]
param (
    [string]$ado_pat, # Azure DevOps PAT
    [string]$ado_org, # Azure devops org without the URL, eg: "MyAzureDevOpsOrg"
    [string]$ado_project, # Team project name that contains the work items, eg: "TailWindTraders"
    [string]$ado_area_path, # Area path in Azure DevOps to migrate; uses the 'UNDER' operator)
    [string]$gh_pat, # GitHub PAT
    [string]$gh_org, # GitHub organization to create the issues in
    [string]$gh_repo, # Default gitHub repository to create the issues in. This gets overridden using the value in the title
    [switch]$gh_update_assigned_to, # try to update the assigned to field in GitHub
    [string]$gh_assignee = "", # github user to assign to
    [string]$gh_assigned_to_user_suffix = "", # the emu suffix, ie: "_corp"
    [switch]$gh_add_ado_comments # try to get ado comments
)

# Set the auth token for az commands
$env:AZURE_DEVOPS_EXT_PAT = $ado_pat;
# Set the auth token for gh commands
$env:GH_TOKEN = $gh_pat;

az devops configure --defaults organization="https://dev.azure.com/$ado_org" project="$ado_project";

$wiql = "select [ID], [Title], [System.Tags] from workitems where [State] <> 'Done' and [State] <> 'Closed' and [State] <> 'Resolved' and [State] <> 'Removed' and [System.AreaPath] UNDER '$ado_area_path' and [System.Title] Contains 'CodeQL' and not [System.Tags] Contains 'copied-to-github' order by [ID]";

$query = az boards query --wiql $wiql | ConvertFrom-Json;

Remove-Item -Path ./temp_comment_body.txt -ErrorAction SilentlyContinue;
Remove-Item -Path ./temp_issue_body.txt -ErrorAction SilentlyContinue;
$count = 0;

ForEach($workitem in $query) {
    $original_workitem_json_beginning = "`n`n<details><summary>Original Work Item JSON</summary><p>" + "`n`n" + '```json';
    $original_workitem_json_end = "`n" + '```' + "`n</p></details>";

    $workitemId = $workitem.id;
    $workitemTags = $workitem.fields.'System.Tags';

    $details_json = az boards work-item show --id $workitem.id --output json;
    $details = $details_json | ConvertFrom-Json;

    # double quotes in the title must be escaped with \ to be passed to gh cli
    $title = $details.fields.{System.Title} -replace "`"","`\`"";

    # get the target github repo from the end of the work item title.
    $index = $title.LastIndexOf('/');
    if($index -gt 0) {
        $repo = $title.Substring($index + 1);
        if(![string]::IsNullOrEmpty($repo.Trim())) {
            $gh_repo = $repo;
        }
    }

    Write-Host "Copying work item $workitemId to $gh_org/$gh_repo on github";

    $description="";

    # bug doesn't have Description field - add repro steps and/or system info
    if ($details.fields.{System.WorkItemType} -eq "Bug") {
        if(![string]::IsNullOrEmpty($details.fields.{Microsoft.VSTS.TCM.ReproSteps})) {
            # Fix line # reference in "Repository:" URL.
            $reproSteps = ($details.fields.{Microsoft.VSTS.TCM.ReproSteps}).Replace('/tree/', '/blob/').Replace('?&amp;path=', '').Replace('&amp;line=', '#L');
            $description += "## Repro Steps`n`n" + $reproSteps + "`n`n";
        }
        if(![string]::IsNullOrEmpty($details.fields.{Microsoft.VSTS.TCM.SystemInfo})) {
            $workitemUrl = "https://dev.azure.com/$ado_org/$ado_project/_workitems/edit/$workitemId";
            $copiedFromNotice = "This issue is a copy of [ADO work item $workitemId]($workitemUrl) created by CodeQL.";
            $description+="## System Info`n`n" + $copiedFromNotice + "`n" + $details.fields.{Microsoft.VSTS.TCM.SystemInfo} + "`n`n";
        }
    } else {
        $description+=$details.fields.{System.Description};
        # add in acceptance criteria if it has it
        if(![string]::IsNullOrEmpty($details.fields.{Microsoft.VSTS.Common.AcceptanceCriteria})) {
            $description+="`n`n## Acceptance Criteria`n`n" + $details.fields.{Microsoft.VSTS.Common.AcceptanceCriteria};
        }
    }

    $description | Out-File -FilePath ./temp_issue_body.txt -Encoding ASCII;

    $url="[Original Work Item URL](https://dev.azure.com/$ado_org/$ado_project/_workitems/edit/$($workitem.id))";
    $url | Out-File -FilePath ./temp_comment_body.txt -Encoding ASCII;

    # use empty string if there is no user is assigned
    if ( $null -ne $details.fields.{System.AssignedTo}.displayName )
    {
        $ado_assigned_to_display_name = $details.fields.{System.AssignedTo}.displayName;
        $ado_assigned_to_unique_name = $details.fields.{System.AssignedTo}.uniqueName;
    }
    else {
        $ado_assigned_to_display_name = "";
        $ado_assigned_to_unique_name = "";
    }
    
    # create the details table
    $ado_details_beginning="`n`n<details><summary>Original Work Item Details</summary><p>" + "`n`n";
    $ado_details_beginning | Add-Content -Path ./temp_comment_body.txt -Encoding ASCII;
    $ado_details= "| Created date | Created by | Changed date | Changed By | Assigned To | State | Type | Area Path | Iteration Path|`n|---|---|---|---|---|---|---|---|---|`n";
    $ado_details+="| $($details.fields.{System.CreatedDate}) | $($details.fields.{System.CreatedBy}.displayName) | $($details.fields.{System.ChangedDate}) | `
        $($details.fields.{System.ChangedBy}.displayName) | $ado_assigned_to_display_name | $($details.fields.{System.State}) | $($details.fields.{System.WorkItemType}) | `
        $($details.fields.{System.AreaPath}) | $($details.fields.{System.IterationPath}) |`n`n";
    $ado_details | Add-Content -Path ./temp_comment_body.txt -Encoding ASCII;
    $ado_details_end="`n" + "`n</p></details>";
    $ado_details_end | Add-Content -Path ./temp_comment_body.txt -Encoding ASCII;

    # prepare the comment
    $original_workitem_json_beginning | Add-Content -Path ./temp_comment_body.txt -Encoding ASCII;
    $details_json | Add-Content -Path ./temp_comment_body.txt -Encoding ASCII;
    $original_workitem_json_end | Add-Content -Path ./temp_comment_body.txt -Encoding ASCII;

    # get comments if enabled
    if($gh_add_ado_comments) {
        $headers = New-Object "System.Collections.Generic.Dictionary[[String],[String]]";
        $base64 = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes(":$ado_pat"));
        $headers.Add("Authorization", "Basic $base64");
        $response = Invoke-RestMethod "https://dev.azure.com/$ado_org/$ado_project/_apis/wit/workItems/$($workitem.id)/comments?api-version=7.1-preview.3" -Method 'GET' -Headers $headers;
        
        if($response.count -gt 0) {
            $ado_comments_details="";
            $ado_original_workitem_json_beginning="`n`n<details><summary>Work Item Comments ($($response.count))</summary><p>" + "`n`n";
            $ado_original_workitem_json_beginning | Add-Content -Path ./temp_comment_body.txt -Encoding ASCII;
            ForEach($comment in $response.comments) {
                $ado_comments_details= "| Created date | Created by | JSON URL |`n|---|---|---|`n";
                $ado_comments_details+="| $($comment.createdDate) | $($comment.createdBy.displayName) | [URL]($($comment.url)) |`n`n";
                $ado_comments_details+="**Comment text**: $($comment.text)`n`n-----------`n`n";
                $ado_comments_details | Add-Content -Path ./temp_comment_body.txt -Encoding ASCII;
            }
            $ado_original_workitem_json_end="`n" + "`n</p></details>";
            $ado_original_workitem_json_end | Add-Content -Path ./temp_comment_body.txt -Encoding ASCII;
        }
    }
    
    # set the label on the issue to be the work item type
    $work_item_type = $details.fields.{System.WorkItemType}.ToLower();

    # create the issue in Github
    $issue_url = gh issue create --body-file ./temp_issue_body.txt --repo "$gh_org/$gh_repo" --title "$title" --label $work_item_type;
    
    if (![string]::IsNullOrEmpty($issue_url.Trim())) {
        Write-Host "  Issue created: $issue_url";
        $count++;
    }
    else {
        throw "Issue creation failed.";
    }
    
    # update assigned to in GitHub if the option is set - tries to use ado email to map to github username
    if ($gh_update_assigned_to) {
        write-host "  Assigning issue to: $gh_assignee";
        $assigned=gh issue edit $issue_url --add-assignee "$gh_assignee";
    }

    # add the comment
    $comment_url=gh issue comment $issue_url --body-file ./temp_comment_body.txt;
    write-host "  Comment created: $comment_url";

    Remove-Item -Path ./temp_comment_body.txt -ErrorAction SilentlyContinue;
    Remove-Item -Path ./temp_issue_body.txt -ErrorAction SilentlyContinue;

    # Add the tag "copied-to-github" plus a comment to the work item
    $discussion = "This work item was copied to github as issue <a href=`"$issue_url`">$issue_url</a>";
    az boards work-item update --id "$workitemId" --fields "System.Tags=copied-to-github; $workitemTags" --discussion "$discussion" | Out-Null;
}
Write-Host "Total items copied: $count"