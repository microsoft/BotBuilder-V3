var assert = require('assert');
var builder = require('../');

describe('Library', function() {
    it ('should have BotBuilder exports', function (done) {
        assert(builder);
        assert(builder.Session);
        assert(builder.Dialog);
        assert(builder.ResumeReason);
        assert(builder.DialogAction);
        assert(builder.DialogCollection);
        assert(builder.PromptType);
        assert(builder.ListStyle);
        assert(builder.Prompts);
        assert(builder.SimplePromptRecognizer);
        assert(builder.IntentDialog);
        assert(builder.IntentGroup);
        assert(builder.LuisDialog);
        assert(builder.CommandDialog);
        assert(builder.EntityRecognizer);
        assert(builder.MemoryStorage);
        assert(builder.BotConnectorBot);
        assert(builder.BotConnectorSession);
        assert(builder.SkypeBot);
        assert(builder.SkypeSession);
        assert(builder.SlackBot);
        assert(builder.SlackSession);
        assert(builder.TextBot);
        done();
    });
});
