var builder = require('../../core/');

//=========================================================
// Library creation
//=========================================================

var lib = new builder.Library('profile');

// Export createLibrary() function
exports.createLibrary = function () {
    return lib.clone();
}

//=========================================================
// Change Name Prompt
//=========================================================

exports.changeName = function (session) {
    session.beginDialog('profile:changeName');
}

lib.dialog('changeName', [
    function (session) {
        session.dialogData.updating = session.userData.hasOwnProperty('name');
        var prompt =  session.dialogData.updating ? "What would you like to change your name to?" : "Hi... What's your name?";
        builder.Prompts.text(session, prompt);
    },
    function (session, results) {
        session.userData.name = results.response;
        if (session.dialogData.updating) {
            session.endDialog("Got it %s...", session.userData.name);
        } else {
            session.endDialog();
        }
    }    
]).triggerAction({ matches: /(change|set|update).*name/i });
