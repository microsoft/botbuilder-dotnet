# Bot Builder SDK v4 - Transcript Files

This folder contains the transcript files (.chat files) used in BotBuilder-v4 for testing several components and guarantee the same behavior between the different platform (.NET, NodeJS, Python, Java).

## BotBuilder Developer writing new transcript-based tests

1. Clone/Fork the [BotBuilder repository](https://github.com/Microsoft/BotBuilder).

2. Create a branch and place the new transcripts in the [Common\Transcripts folder](https://github.com/southworkscom/BotBuilder/tree/botbuilder-v4-transcripts/Common/Transcripts) (this can be done locally without actually pushing changes at this stage)

3. Write code and create new tests. Set the `BOTBUILDER_TRANSCRIPTS_LOCATION` Environment var pointing to the local repo / transcript folder.

    E.g.:

    ```shell
    # On *nix:
    export BOTBUILDER_TRANSCRIPTS_LOCATION=~/projects/BotBuilder/Common/Transcripts
    ```


    ```Batchfile
    REM On Windows:
    SET BOTBUILDER_TRANSCRIPTS_LOCATION=C:\Projects\BotBuilder\Common\Transcripts
    ```

4. Run the transcript tests from the platform repository. The code will look into the BOTBUILDER_TRANSCRIPTS_LOCATION folder for all files.

    E.g.:

    ```
    cd transcripts
    npm run test
    ```

## Pushing changes to BotBuilder Repos

1. Create a PR to the [BotBuilder repository](https://github.com/Microsoft/BotBuilder) with the new transcripts.

2. Once the transcripts PR is merged, create a PR to the BotBuilder's platform repository with the code updates and tests.
