/*-----------------------------------------------------------------------------
This Bot demonstrates how to use Bongs Dialog Manager service.
-----------------------------------------------------------------------------*/

var builder = require('../../core/');

var connector = new builder.ConsoleConnector().listen();
var bot = new builder.UniversalBot(connector);

// Create DialogManager instance
var dialog = new builder.DialogManager({
    agentId: process.env.BING_AGENT_ID,
    appId: process.env.BING_APP_ID
});
bot.dialog('/', dialog);

dialog.onDefault(function (session) { 
    session.send("I'm sorry I didn't understand.");
});
