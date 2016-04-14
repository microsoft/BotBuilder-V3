var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var dialog = require('./Dialog');
var consts = require('../consts');
var entities = require('./EntityRecognizer');
(function (PromptType) {
    PromptType[PromptType["text"] = 0] = "text";
    PromptType[PromptType["number"] = 1] = "number";
    PromptType[PromptType["confirm"] = 2] = "confirm";
    PromptType[PromptType["choice"] = 3] = "choice";
    PromptType[PromptType["time"] = 4] = "time";
})(exports.PromptType || (exports.PromptType = {}));
var PromptType = exports.PromptType;
(function (ListStyle) {
    ListStyle[ListStyle["none"] = 0] = "none";
    ListStyle[ListStyle["inline"] = 1] = "inline";
    ListStyle[ListStyle["list"] = 2] = "list";
})(exports.ListStyle || (exports.ListStyle = {}));
var ListStyle = exports.ListStyle;
var SimplePromptRecognizer = (function () {
    function SimplePromptRecognizer() {
        this.cancelExp = /^(cancel|nevermind|never mind|back|stop|forget it)/i;
    }
    SimplePromptRecognizer.prototype.recognize = function (args, callback, session) {
        this.checkCanceled(args, function () {
            try {
                var score = 0.0;
                var response;
                var text = args.utterance.trim();
                switch (args.promptType) {
                    case PromptType.text:
                        score = 0.1;
                        response = text;
                        break;
                    case PromptType.number:
                        var n = entities.EntityRecognizer.parseNumber(text);
                        if (!isNaN(n)) {
                            var score = n.toString().length / text.length;
                            response = n;
                        }
                        break;
                    case PromptType.confirm:
                        var b = entities.EntityRecognizer.parseBoolean(text);
                        if (typeof b == 'boolean') {
                            score = 1.0;
                            response = b;
                        }
                        break;
                    case PromptType.time:
                        var entity = entities.EntityRecognizer.recognizeTime(text, args.refDate ? new Date(args.refDate) : null);
                        if (entity) {
                            score = entity.entity.length / text.length;
                            response = entity;
                        }
                        break;
                    case PromptType.choice:
                        var best = entities.EntityRecognizer.findBestMatch(args.enumValues, text);
                        if (!best) {
                            var n = entities.EntityRecognizer.parseNumber(text);
                            if (!isNaN(n) && n > 0 && n <= args.enumValues.length) {
                                best = { index: n, entity: args.enumValues[n - 1], score: 1.0 };
                            }
                        }
                        if (best) {
                            score = best.score;
                            response = best;
                        }
                        break;
                    default:
                }
                args.compareConfidence(args.language, text, score, function (handled) {
                    if (!handled && score > 0) {
                        callback({ resumed: dialog.ResumeReason.completed, promptType: args.promptType, response: response });
                    }
                    else {
                        callback({ resumed: dialog.ResumeReason.notCompleted, promptType: args.promptType, handled: handled });
                    }
                });
            }
            catch (err) {
                callback({ resumed: dialog.ResumeReason.notCompleted, promptType: args.promptType, error: err instanceof Error ? err : new Error(err.toString()) });
            }
        }, callback);
    };
    SimplePromptRecognizer.prototype.checkCanceled = function (args, onContinue, callback) {
        if (!this.cancelExp.test(args.utterance.trim())) {
            onContinue();
        }
        else {
            callback({ resumed: dialog.ResumeReason.canceled, promptType: args.promptType });
        }
    };
    return SimplePromptRecognizer;
})();
exports.SimplePromptRecognizer = SimplePromptRecognizer;
var Prompts = (function (_super) {
    __extends(Prompts, _super);
    function Prompts() {
        _super.apply(this, arguments);
    }
    Prompts.prototype.begin = function (session, args) {
        args = args || {};
        args.maxRetries = args.maxRetries || 1;
        for (var key in args) {
            if (args.hasOwnProperty(key)) {
                session.dialogData[key] = args[key];
            }
        }
        session.send(args.prompt);
    };
    Prompts.prototype.replyReceived = function (session) {
        var args = session.dialogData;
        Prompts.options.recognizer.recognize({
            promptType: args.promptType,
            utterance: session.message.text,
            language: session.message.language,
            enumValues: args.enumValues,
            refDate: args.refDate,
            compareConfidence: function (language, utterance, score, callback) {
                session.compareConfidence(language, utterance, score, callback);
            }
        }, function (result) {
            if (!result.handled) {
                if (result.error || result.resumed == dialog.ResumeReason.completed ||
                    result.resumed == dialog.ResumeReason.canceled || args.maxRetries == 0) {
                    result.promptType = args.promptType;
                    session.endDialog(result);
                }
                else {
                    args.maxRetries--;
                    session.send(args.retryPrompt || "I didn't understand. " + args.prompt);
                }
            }
        });
    };
    Prompts.configure = function (options) {
        if (options) {
            for (var key in options) {
                if (options.hasOwnProperty(key)) {
                    Prompts.options[key] = options[key];
                }
            }
        }
    };
    Prompts.text = function (ses, prompt) {
        beginPrompt(ses, {
            promptType: PromptType.text,
            prompt: prompt
        });
    };
    Prompts.number = function (ses, prompt, options) {
        var args = options || {};
        args.promptType = PromptType.number;
        args.prompt = prompt;
        beginPrompt(ses, args);
    };
    Prompts.confirm = function (ses, prompt, options) {
        var args = options || {};
        args.promptType = PromptType.confirm;
        args.prompt = prompt;
        beginPrompt(ses, args);
    };
    Prompts.choice = function (ses, prompt, choices, options) {
        var args = options || {};
        args.promptType = PromptType.choice;
        args.prompt = prompt;
        args.enumValues = entities.EntityRecognizer.expandChoices(choices);
        args.listStyle = args.listStyle || ListStyle.list;
        var connector = '', list;
        switch (args.listStyle) {
            case ListStyle.list:
                list = '\n   ';
                args.enumValues.forEach(function (value, index) {
                    list += connector + (index + 1) + '. ' + value;
                    connector = '\n   ';
                });
                args.prompt += list;
                break;
            case ListStyle.inline:
                list = ' ';
                args.enumValues.forEach(function (value, index) {
                    list += connector + (index + 1) + '. ' + value;
                    if (index == args.enumValues.length - 2) {
                        connector = index == 0 ? ' or ' : ', or ';
                    }
                    else {
                        connector = ', ';
                    }
                });
                args.prompt += list;
                break;
        }
        beginPrompt(ses, args);
    };
    Prompts.time = function (ses, prompt, options) {
        var args = options || {};
        args.promptType = PromptType.time;
        args.prompt = prompt;
        beginPrompt(ses, args);
    };
    Prompts.options = {
        recognizer: new SimplePromptRecognizer()
    };
    return Prompts;
})(dialog.Dialog);
exports.Prompts = Prompts;
function beginPrompt(ses, args) {
    ses.beginDialog(consts.DialogId.Prompts, args);
}
