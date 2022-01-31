// This validates parity labels for a PR. See const validLabels below.

const core = require(`@actions/core`);
const github = require(`@actions/github`);

const getPullRequestNumber = (ref) => {
  core.debug(`Parsing ref: ${ref}`);
  // This assumes that the ref is in the form of `refs/pull/:prNumber/merge`
  const prNumber = ref.replace(/refs\/pull\/(\d+)\/merge/, `$1`);
  return parseInt(prNumber, 10);
};

(async () => {
  try {
    const owner = github.context.repo.owner;
    const repo = github.context.repo.repo;
    const ref = github.context.ref;
    const prNumber = github.context.issue.number || getPullRequestNumber(ref);
    const gitHubToken = core.getInput(`github-token`, { required: true });
    const octokit = new github.getOctokit(gitHubToken);

    const validLabels = [
      `Automation: No parity`, 
      `Automation: Parity all`, 
      `Automation: Parity with dotnet`, 
      `Automation: Parity with Java`, 
      `Automation: Parity with JS`, 
      `Automation: Parity with Python`];

    const getPrLabels = async (prNumber) => {
      const { data } = await octokit.pulls.get({
        pull_number: prNumber,
        owner,
        repo,
      });
      if (data.length === 0) {
        throw new Error(`No Pull Requests found for ${prNumber} (${ref}).`);
      }
      return data.labels.map((label) => label.name);
    };

    const prLabels = await getPrLabels(prNumber);
    core.debug(`Found PR labels: ${prLabels.toString()}`);

    // Get the valid parity labels in this pull request.
    const prValidLabels = prLabels.filter(value => validLabels.includes(value));

    if (prValidLabels.length > 0) {
      core.info(`OK: Pull Request has at least one parity label.`);
    }
    else {
      core.error(`Required is at least one of these labels: ${validLabels.join(`, `)}`);
      throw `no labels`;
    }

    // Ensure no other parity labels accompany a `no parity` or a `parity all` label.
    const parityLabelConflict = prValidLabels.find(element => {
      if ( (element.toLowerCase().includes(`no parity`) || 
          element.toLowerCase().includes(`parity all`)) && 
          prValidLabels.length > 1) {
        return true;
      }
    });

    if (parityLabelConflict == null) {
      core.info(`OK: No parity label conflict.`);
    }
    else {
      core.error(`Label ${parityLabelConflict} must not accompany other parity labels: ${prValidLabels.join(`, `)}`);
      throw `parity label conflict`;
    }

    // Ensure a parity label does not target this repo.
    // Example: For a PR in the repo botbuilder-js, label `Automation: Parity with JS` is not allowed.
    // This checks that the repo name does not contain the label`s last word.
    const labelTargetingRepo = prValidLabels.find(element => {
      var splitString = element.split(` `);
      var lastWord = splitString[splitString.length - 1];
      if ( repo.toLowerCase().includes(lastWord.toLowerCase())) {
        return true;
      }
    });

    if (labelTargetingRepo == null) {
      core.info(`OK: No parity labels target this repo.`);
    }
    else {
      core.error(`This parity label is not allowed because it targets the ${repo} repo: ${labelTargetingRepo}`);
      throw `forbidden label`;
    }

    return 0;
  } catch (error) {
    await core.setFailed(error.stack || error.message);
  }
})();
