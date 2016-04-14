var builder = require('../../../');

module.exports = {
    description: "Sends an image",
    async: true,
    addDialogs: addDialogs,
    run: run 
};

function addDialogs(bot) {
    bot.add('/tests/send-attachment', function (session) {
        var attribution = "RobotsMODO By AlejandroLinaresGarcia (Own work) [CC BY-SA 3.0 (http://creativecommons.org/licenses/by-sa/3.0)], via Wikimedia Commons";
        var imageLink = 'https://upload.wikimedia.org/wikipedia/commons/d/df/RobotsMODO.jpg';
        var reply = new builder.Message()
                               .setText(session, attribution)
                               .addAttachment({ fallbackText: attribution, contentType: 'image/jpeg', contentUrl: imageLink });
        session.endDialog(reply);
    });
}

function run(session) {
    session.beginDialog('/tests/send-attachment');
}
