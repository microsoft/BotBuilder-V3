"use strict";
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var Prompt_1 = require("./Prompt");
var PromptRecognizers_1 = require("./PromptRecognizers");
var consts = require("../consts");
var PromptNumber = (function (_super) {
    __extends(PromptNumber, _super);
    function PromptNumber(features) {
        var _this = _super.call(this, {
            defaultRetryPrompt: 'default_number',
            defaultRetryNamespace: consts.Library.system
        }) || this;
        _this.updateFeatures(features);
        _this.onRecognize(function (context, cb) {
            if (context.message.text && !_this.features.disableRecognizer) {
                var options = context.dialogData.options;
                var entities = PromptRecognizers_1.PromptRecognizers.recognizeNumbers(context, options);
                var top_1 = PromptRecognizers_1.PromptRecognizers.findTopEntity(entities);
                if (top_1) {
                    cb(null, top_1.score, top_1.entity);
                }
                else {
                    cb(null, 0.0);
                }
            }
            else {
                cb(null, 0.0);
            }
        });
        _this.matches(consts.Intents.Repeat, function (session) {
            session.dialogData.turns = 0;
            _this.sendPrompt(session);
        });
        return _this;
    }
    return PromptNumber;
}(Prompt_1.Prompt));
exports.PromptNumber = PromptNumber;
