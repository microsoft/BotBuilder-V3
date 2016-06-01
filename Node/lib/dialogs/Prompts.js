var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var dialog = require('./Dialog');
var consts = require('../consts');
var entities = require('./EntityRecognizer');
var mb = require('../Message');
var Channel = require('../Channel');
var dc = require('./DialogCollection');
(function (PromptType) {
    PromptType[PromptType["text"] = 0] = "text";
    PromptType[PromptType["number"] = 1] = "number";
    PromptType[PromptType["confirm"] = 2] = "confirm";
    PromptType[PromptType["choice"] = 3] = "choice";
    PromptType[PromptType["time"] = 4] = "time";
    PromptType[PromptType["attachment"] = 5] = "attachment";
})(exports.PromptType || (exports.PromptType = {}));
var PromptType = exports.PromptType;
(function (ListStyle) {
    ListStyle[ListStyle["none"] = 0] = "none";
    ListStyle[ListStyle["inline"] = 1] = "inline";
    ListStyle[ListStyle["list"] = 2] = "list";
    ListStyle[ListStyle["button"] = 3] = "button";
    ListStyle[ListStyle["auto"] = 4] = "auto";
})(exports.ListStyle || (exports.ListStyle = {}));
var ListStyle = exports.ListStyle;
var SimplePromptRecognizer = (function () {
    function SimplePromptRecognizer() {
        this.cancelExp = /^(cancel|nevermind|never mind|stop|forget it|quit)/i;
    }
    SimplePromptRecognizer.prototype.recognize = function (args, callback, session) {
        this.checkCanceled(args, function () {
            try {
                var score = 0.0;
                var response;
                var text = args.utterance.trim();
                switch (args.promptType) {
                    default:
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
                        if (typeof b !== 'boolean') {
                            var n = entities.EntityRecognizer.parseNumber(text);
                            if (!isNaN(n) && n > 0 && n <= 2) {
                                b = (n === 1);
                            }
                        }
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
                                best = { index: n - 1, entity: args.enumValues[n - 1], score: 1.0 };
                            }
                        }
                        if (best) {
                            score = best.score;
                            response = best;
                        }
                        break;
                    case PromptType.attachment:
                        if (args.attachments && args.attachments.length > 0) {
                            score = 1.0;
                            response = args.attachments;
                        }
                        break;
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
        this.sendPrompt(session, args);
    };
    Prompts.prototype.replyReceived = function (session) {
        var _this = this;
        var args = session.dialogData;
        Prompts.options.recognizer.recognize({
            promptType: args.promptType,
            utterance: session.message.text,
            language: session.message.language,
            attachments: session.message.attachments,
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
                    _this.sendPrompt(session, args, true);
                }
            }
        });
    };
    Prompts.prototype.sendPrompt = function (session, args, retry) {
        if (retry === void 0) { retry = false; }
        if (retry && typeof args.retryPrompt === 'object' && !Array.isArray(args.retryPrompt)) {
            session.send(args.retryPrompt);
        }
        else if (typeof args.prompt === 'object' && !Array.isArray(args.prompt)) {
            session.send(args.prompt);
        }
        else {
            var style = ListStyle.none;
            if (args.promptType == PromptType.choice || args.promptType == PromptType.confirm) {
                style = args.listStyle;
                if (style == ListStyle.auto) {
                    if (Channel.preferButtons(session, args.enumValues.length, retry)) {
                        style = ListStyle.button;
                    }
                    else if (!retry) {
                        style = args.enumValues.length < 3 ? ListStyle.inline : ListStyle.list;
                    }
                    else {
                        style = ListStyle.none;
                    }
                }
            }
            var prompt;
            if (retry) {
                if (args.retryPrompt) {
                    prompt = mb.Message.randomPrompt(args.retryPrompt);
                }
                else {
                    var type = PromptType[args.promptType];
                    prompt = mb.Message.randomPrompt(Prompts.defaultRetryPrompt[type]);
                }
            }
            else {
                prompt = mb.Message.randomPrompt(args.prompt);
            }
            var connector = '';
            var list;
            var msg = new mb.Message();
            switch (style) {
                case ListStyle.button:
                    var a = { actions: [] };
                    for (var i = 0; i < session.dialogData.enumValues.length; i++) {
                        var action = session.dialogData.enumValues[i];
                        a.actions.push({ title: action, message: action });
                    }
                    msg.setText(session, prompt)
                        .addAttachment(a);
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
                    msg.setText(session, prompt + '%s', list);
                    break;
                case ListStyle.list:
                    list = '\n   ';
                    args.enumValues.forEach(function (value, index) {
                        list += connector + (index + 1) + '. ' + value;
                        connector = '\n   ';
                    });
                    msg.setText(session, prompt + '%s', list);
                    break;
                default:
                    msg.setText(session, prompt);
                    break;
            }
            session.send(msg);
        }
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
    Prompts.text = function (session, prompt) {
        beginPrompt(session, {
            promptType: PromptType.text,
            prompt: prompt
        });
    };
    Prompts.number = function (session, prompt, options) {
        var args = options || {};
        args.promptType = PromptType.number;
        args.prompt = prompt;
        beginPrompt(session, args);
    };
    Prompts.confirm = function (session, prompt, options) {
        var args = options || {};
        args.promptType = PromptType.confirm;
        args.prompt = prompt;
        args.enumValues = ['yes', 'no'];
        args.listStyle = args.hasOwnProperty('listStyle') ? args.listStyle : ListStyle.auto;
        beginPrompt(session, args);
    };
    Prompts.choice = function (session, prompt, choices, options) {
        var args = options || {};
        args.promptType = PromptType.choice;
        args.prompt = prompt;
        args.listStyle = args.hasOwnProperty('listStyle') ? args.listStyle : ListStyle.auto;
        args.enumValues = entities.EntityRecognizer.expandChoices(choices);
        beginPrompt(session, args);
    };
    Prompts.time = function (session, prompt, options) {
        var args = options || {};
        args.promptType = PromptType.time;
        args.prompt = prompt;
        beginPrompt(session, args);
    };
    Prompts.attachment = function (session, prompt, options) {
        var args = options || {};
        args.promptType = PromptType.attachment;
        args.prompt = prompt;
        beginPrompt(session, args);
    };
    Prompts.options = {
        recognizer: new SimplePromptRecognizer()
    };
    Prompts.defaultRetryPrompt = {
        text: "I didn't understand. Please try again.",
        number: "I didn't recognize that as a number. Please enter a number.",
        confirm: "I didn't understand. Please answer 'yes' or 'no'.",
        choice: "I didn't understand. Please choose an option from the list.",
        time: "I didn't recognize the time you entered. Please try again.",
        attachment: "I didn't receive a file. Please try again."
    };
    return Prompts;
})(dialog.Dialog);
exports.Prompts = Prompts;
dc.systemDialogs[consts.DialogId.Prompts] = new Prompts();
function beginPrompt(session, args) {
    session.beginDialog(consts.DialogId.Prompts, args);
}
