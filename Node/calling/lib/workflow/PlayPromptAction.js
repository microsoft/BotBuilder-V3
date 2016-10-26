"use strict";
var uuid = require('node-uuid');
var prompt = require('./Prompt');
var PlayPromptAction = (function () {
    function PlayPromptAction(session) {
        this.session = session;
        this.data = {};
        this.data.action = 'playPrompt';
        this.data.operationId = uuid.v4();
    }
    PlayPromptAction.prototype.prompts = function (list) {
        this.data.prompts = [];
        if (list) {
            for (var i = 0; i < list.length; i++) {
                var p = list[i];
                this.data.prompts.push(p.toPrompt ? p.toPrompt() : p);
            }
        }
        return this;
    };
    PlayPromptAction.prototype.toAction = function () {
        return this.data;
    };
    PlayPromptAction.text = function (session, text) {
        var args = [];
        for (var _i = 2; _i < arguments.length; _i++) {
            args[_i - 2] = arguments[_i];
        }
        args.unshift(text);
        var p = new prompt.Prompt(session);
        prompt.Prompt.prototype.value.apply(p, args);
        return new PlayPromptAction(session).prompts([p]);
    };
    PlayPromptAction.file = function (session, uri) {
        return new PlayPromptAction(session).prompts([prompt.Prompt.file(session, uri)]);
    };
    PlayPromptAction.silence = function (session, time) {
        return new PlayPromptAction(session).prompts([prompt.Prompt.silence(session, time)]);
    };
    return PlayPromptAction;
}());
exports.PlayPromptAction = PlayPromptAction;
