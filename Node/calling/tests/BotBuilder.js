var assert = require('assert');
var builder = require('../');

describe('Library', function() {
    it ('should have BotBuilder exports', function (done) {
        assert(builder.CallSession);
        assert(builder.CallState);
        assert(builder.ModalityType);
        assert(builder.NotificationType);
        assert(builder.OperationOutcome);
        assert(builder.AnswerAction);
        assert(builder.HangupAction);
        assert(builder.PlayPromptAction);
        assert(builder.Prompt);
        assert(builder.SayAs);
        assert(builder.VoiceGender);
        assert(builder.RecognizeAction);
        assert(builder.RecognitionCompletionReason);
        assert(builder.DigitCollectionCompletionReason);
        assert(builder.RecordAction);
        assert(builder.RecordingCompletionReason);
        assert(builder.RecordingFormat);
        assert(builder.RejectAction);
        assert(builder.Dialog);
        assert(builder.ResumeReason);
        assert(builder.DialogAction);
        assert(builder.PromptType);
        assert(builder.Prompts);
        assert(builder.SimpleDialog);
        assert(builder.UniversalCallBot);
        assert(builder.Library);
        assert(builder.CallConnector);
        assert(builder.MemoryBotStorage);
        done();
    });
});
