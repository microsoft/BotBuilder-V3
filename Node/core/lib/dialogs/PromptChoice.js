"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const Prompt_1 = require("./Prompt");
const PromptRecognizers_1 = require("./PromptRecognizers");
const Message_1 = require("../Message");
const Keyboard_1 = require("../cards/Keyboard");
const consts = require("../consts");
const Channel = require("../Channel");
class PromptChoice extends Prompt_1.Prompt {
    constructor(features) {
        super({
            defaultRetryPrompt: 'default_choice',
            defaultRetryNamespace: consts.Library.system,
            recognizeNumbers: true,
            recognizeOrdinals: true,
            recognizeChoices: true,
            defaultListStyle: Prompt_1.ListStyle.list,
            inlineListCount: 3,
            minScore: 0.4
        });
        this._onChoices = [];
        this.updateFeatures(features);
        this.onRecognize((context, cb) => {
            if (context.message.text && !this.features.disableRecognizer) {
                this.findChoices(context, true, (err, choices) => {
                    if (err || !choices || !choices.length) {
                        return cb(err, 0.0);
                    }
                    let topScore = 0.0;
                    let topMatch = null;
                    let utterance = context.message.text.trim();
                    if (this.features.recognizeChoices) {
                        let options = { allowPartialMatches: true };
                        let match = PromptRecognizers_1.PromptRecognizers.findTopEntity(PromptRecognizers_1.PromptRecognizers.recognizeChoices(utterance, choices, options));
                        if (match) {
                            topScore = match.score;
                            topMatch = match.entity;
                        }
                    }
                    if (this.features.recognizeNumbers) {
                        let options = { minValue: 1, maxValue: choices.length, integerOnly: true };
                        let match = PromptRecognizers_1.PromptRecognizers.findTopEntity(PromptRecognizers_1.PromptRecognizers.recognizeNumbers(context, options));
                        if (match && match.score > topScore) {
                            let index = Math.floor(match.entity - 1);
                            topScore = match.score;
                            topMatch = {
                                score: match.score,
                                index: index,
                                entity: choices[index].value
                            };
                        }
                    }
                    if (this.features.recognizeOrdinals) {
                        let match = PromptRecognizers_1.PromptRecognizers.findTopEntity(PromptRecognizers_1.PromptRecognizers.recognizeOrdinals(context));
                        if (match && match.score > topScore) {
                            let index = match.entity > 0 ? match.entity - 1 : choices.length + match.entity;
                            if (index >= 0 && index < choices.length) {
                                topScore = match.score;
                                topMatch = {
                                    score: match.score,
                                    index: index,
                                    entity: choices[index].value
                                };
                            }
                        }
                    }
                    if (topScore >= this.features.minScore && topScore > 0) {
                        cb(null, topScore, topMatch);
                    }
                    else {
                        cb(null, 0.0);
                    }
                });
            }
            else {
                cb(null, 0.0);
            }
        });
        this.onFormatMessage((session, text, speak, callback) => {
            let context = session.dialogData;
            let options = context.options;
            this.findChoices(session.toRecognizeContext(), false, (err, choices) => {
                let msg;
                if (!err && choices) {
                    let sendChoices = context.turns === 0 || context.isReprompt;
                    let listStyle = options.listStyle;
                    if (listStyle === undefined || listStyle === null || listStyle === Prompt_1.ListStyle.auto) {
                        let maxTitleLength = 0;
                        choices.forEach((choice) => {
                            let l = choice.action && choice.action.title ? choice.action.title.length : choice.value.length;
                            if (l > maxTitleLength) {
                                maxTitleLength = l;
                            }
                        });
                        let supportsKeyboards = Channel.supportsKeyboards(session, choices.length);
                        let supportsCardActions = Channel.supportsCardActions(session, choices.length);
                        let maxActionTitleLength = Channel.maxActionTitleLength(session);
                        let hasMessageFeed = Channel.hasMessageFeed(session);
                        if (maxTitleLength <= maxActionTitleLength &&
                            (supportsKeyboards || (!hasMessageFeed && supportsCardActions))) {
                            listStyle = Prompt_1.ListStyle.button;
                            sendChoices = true;
                        }
                        else {
                            listStyle = this.features.defaultListStyle;
                            let inlineListCount = this.features.inlineListCount;
                            if (listStyle === Prompt_1.ListStyle.list && inlineListCount > 0 && choices.length <= inlineListCount) {
                                listStyle = Prompt_1.ListStyle.inline;
                            }
                        }
                    }
                    msg = PromptChoice.formatMessage(session, listStyle, text, speak, sendChoices ? choices : null);
                }
                callback(err, msg);
            });
        });
        this.matches(consts.Intents.Repeat, (session) => {
            session.dialogData.turns = 0;
            this.sendPrompt(session);
        });
    }
    findChoices(context, recognizePhrase, callback) {
        let idx = 0;
        const handlers = this._onChoices;
        function next(err, choices) {
            if (err || choices) {
                callback(err, choices);
            }
            else {
                try {
                    if (idx < handlers.length) {
                        handlers[idx++](context, next, recognizePhrase);
                    }
                    else {
                        choices = context.dialogData.options.choices || [];
                        callback(null, choices);
                    }
                }
                catch (e) {
                    callback(e, null);
                }
            }
        }
        next(null, null);
    }
    onChoices(handler) {
        this._onChoices.unshift(handler);
        return this;
    }
    static formatMessage(session, listStyle, text, speak, choices) {
        let options = session.dialogData.options;
        let locale = session.preferredLocale();
        let namespace = options ? options.libraryNamespace : null;
        choices = choices ? choices : options.choices;
        let msg = new Message_1.Message(session);
        if (speak) {
            msg.speak(session.localizer.gettext(locale, Message_1.Message.randomPrompt(speak), namespace));
        }
        let txt = session.localizer.gettext(locale, Message_1.Message.randomPrompt(text), namespace);
        if (choices && choices.length > 0) {
            let values = [];
            let actions = [];
            choices.forEach((choice) => {
                if (listStyle == Prompt_1.ListStyle.button) {
                    const ca = choice.action || {};
                    let action = {
                        type: ca.type || 'imBack',
                        title: ca.title || choice.value,
                        value: ca.value || choice.value
                    };
                    if (ca.image) {
                        action.image = ca.image;
                    }
                    actions.push(action);
                }
                else if (choice.action && choice.action.title) {
                    values.push(choice.action.title);
                }
                else {
                    values.push(choice.value);
                }
            });
            let connector = '';
            switch (listStyle) {
                case Prompt_1.ListStyle.button:
                    if (actions.length > 0) {
                        let keyboard = new Keyboard_1.Keyboard().buttons(actions);
                        msg.addAttachment(keyboard);
                    }
                    break;
                case Prompt_1.ListStyle.inline:
                    txt += ' (';
                    values.forEach((v, index) => {
                        txt += connector + (index + 1) + '. ' + v;
                        if (index == (values.length - 2)) {
                            let cid = index == 0 ? 'list_or' : 'list_or_more';
                            connector = Prompt_1.Prompt.gettext(session, cid, consts.Library.system);
                        }
                        else {
                            connector = ', ';
                        }
                    });
                    txt += ')';
                    break;
                case Prompt_1.ListStyle.list:
                    txt += '\n\n   ';
                    values.forEach((v, index) => {
                        txt += connector + (index + 1) + '. ' + v;
                        connector = '\n   ';
                    });
                    break;
            }
        }
        return msg.text(txt).toMessage();
    }
}
exports.PromptChoice = PromptChoice;
