/*

Script arguments:
    * Required:
        - projectUrl
        - pipelineId
        - token
    * Optional:
        - intervalInSeconds (20 by default)

*/

const axios = require('axios');
const minimist = require('minimist');

const args = minimist(process.argv.slice(2));

const apiUrl = `${args.projectUrl}/_apis/pipelines/${args.pipelineId}/runs?api-version=7.0`;

const data = {};

const config = {
    auth: {
        username: 'Basic',
        password: args.token
    }
};

(async () => {
    const run = (
        await axios.post(apiUrl, data, config)
    ).data;

    const webUrl = run._links.web.href;

    console.log(`Pipeline run URL: ${webUrl}`);

    const interval = setInterval(async () => {
        const { state, result } = (
            await axios.get(run.url, config)
        ).data;

        console.log(`Current state: "${state}"`);

        if (state != 'completed') return;

        clearInterval(interval);

        const message = `Pipeline run completed with result "${result}"; URL: ${webUrl}`;

        if (result == 'succeeded') {
            console.log(message);
        } else {
            console.log(`##vso[task.logissue type=error]${message}`);
            console.log('##vso[task.complete result=Failed]');
        }
    }, (args.intervalInSeconds || 20) * 1000);
})();
