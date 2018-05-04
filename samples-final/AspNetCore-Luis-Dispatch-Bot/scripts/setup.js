#!/usr/bin/env node
/**
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */
const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

const args = process.argv.slice(2);
if (args.length < 3) {
    console.error("To setup services for this example");
    console.error("npm start LUIS_AUTHORING_KEY LUIS_REGION QNAMAKER_SUBSCRIPTION");
    process.exit(-1);
}

const luisKey = args[0];
const luisRegion = args[1];
const luisEndpoint = "https://" + luisRegion + ".api.cognitive.microsoft.com/luis/api/v2.0";
const qnakey = args[2];

luisImport('homeautomation.json');
luisImport('weather.json');

var qnakbid = qnaKbId('faq');
if (qnakbid) {
    console.log("Reusing existing Q&A Maker KB faq");
}
else {
    console.log("Importing Q&A Maker KB faq");
}

console.log("Creating dispatch model");
if (fs.existsSync('dispatchSample.dispatch')) {
    fs.unlinkSync('dispatchSample.dispatch');
}
callDispatch('dispatch init -name dispatchSample -luisAuthoringKey ' + luisKey + ' -luisAuthoringRegion ' + luisRegion);
callDispatch('dispatch add -type luis -name homeautomation');
callDispatch('dispatch add -type luis -name weather');
callDispatch('dispatch add -type qna -name faq -key ' + qnakey + ' -id ' + qnakbid);
callDispatch('dispatch create');

function call(cmd) {
    var output;
    try {
        return execSync(cmd);
    }
    catch (err) {
        console.log(cmd);
        console.log(err);
    }
}

function callDispatch(cmd)
{
    return call(cmd + " -dataFolder dispatchSample");
}

function callJSON(cmd) {
    var output;
    try {
        output = execSync(cmd);
        return JSON.parse(output);
    }
    catch (err) {
        console.log(cmd);
        console.log(err);
        console.log(output.toString());
    }
}

function qnaCall(cmd) {
    const fullCmd = 'qnamaker ' + cmd + " --subscriptionKey " + qnakey;
    return callJSON(fullCmd);
}

function qnaKbId(name) {
    var id;
    var kbs = qnaCall('list kbs');
    for (var i = 0; i < kbs.knowledgebases.length; ++i) {
        var kb = kbs.knowledgebases[i];
        if (kb.name === name) {
            id = kb.id;
            break;
        }
    }
    return id;
}

function luisCall(cmd, appId) {
    const fullCmd = 'luis ' + cmd + ' --authoringKey ' + luisKey + ' --endpointBasePath ' + luisEndpoint;
    if (appId) {
        fullCmd += " --appId " + appId;
    }
    return callJSON(fullCmd);
}

function luisID(name) {
    var id;
    var apps = luisCall("list applications");
    for (i = 0; i < apps.length; ++i) {
        var app = apps[i];
        if (app.name === name) {
            id = app.id;
            break;
        }
    }
    return id;
}

function luisImport(file) {
    var name = path.basename(file, '.json');
    var id = luisID(name);
    if (!id) {
        console.log("Importing LUIS app " + name);
        id = luisCall("import application --in " + file).id;
        luisCall("train version --versionId 0.1 --wait", id);
        luisCall("publish version --versionId 0.1", id);
    }
    else {
        console.log("Using existing LUIS app " + name);
    }
}
/*
const luisProcess = exec('luis list applications --authoringKey 0f43266ab91447ec8d705897381478c5 --endpointBastPath https://westus.api.cognitive.microsoft.com/luis/api/v2.0', { stdio: ['pipe', 'pipe', process.stderr] });
luisProcess.stdout.on('data', data => {
    var json = JSON.parse(data);
    console.log(data);
});
*/