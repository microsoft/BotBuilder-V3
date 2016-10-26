"use strict";
var uuid = require('node-uuid');
var utils = require('../utils');
exports.RecognitionCompletionReason = {
    unknown: 'unknown',
    initialSilenceTimeout: 'initialSilenceTimeout',
    incorrectDtmf: 'incorrectDtmf',
    interdigitTimeout: 'interdigitTimeout',
    speechOptionMatched: 'speechOptionMatched',
    dtmfOptionMatched: 'dtmfOptionMatched',
    callTerminated: 'callTerminated',
    temporarySystemFailure: 'temporarySystemFailure'
};
exports.DigitCollectionCompletionReason = {
    unknown: 'unknown',
    initialSilenceTimeout: 'initialSilenceTimeout',
    interdigitTimeout: 'interdigitTimeout',
    completedStopToneDetected: 'completedStopToneDetected',
    callTerminated: 'callTerminated',
    temporarySystemFailure: 'temporarySystemFailure'
};
var RecognizeAction = (function () {
    function RecognizeAction(session) {
        this.session = session;
        this.data = session ? utils.clone(session.recognizeDefaults || {}) : {};
        this.data.action = 'recognize';
        this.data.operationId = uuid.v4();
    }
    RecognizeAction.prototype.playPrompt = function (action) {
        if (action) {
            this.data.playPrompt = action.toAction ? action.toAction() : action;
        }
        return this;
    };
    RecognizeAction.prototype.bargeInAllowed = function (flag) {
        this.data.bargeInAllowed = flag;
        return this;
    };
    RecognizeAction.prototype.culture = function (locale) {
        if (locale) {
            this.data.culture = locale;
        }
        return this;
    };
    RecognizeAction.prototype.initialSilenceTimeoutInSeconds = function (time) {
        if (time) {
            this.data.initialSilenceTimeoutInSeconds = time;
        }
        return this;
    };
    RecognizeAction.prototype.interdigitTimeoutInSeconds = function (time) {
        if (time) {
            this.data.interdigitTimeoutInSeconds = time;
        }
        return this;
    };
    RecognizeAction.prototype.choices = function (list) {
        this.data.choices = list || [];
        return this;
    };
    RecognizeAction.prototype.collectDigits = function (options) {
        if (options) {
            this.data.collectDigits = options;
        }
        return this;
    };
    RecognizeAction.prototype.toAction = function () {
        return this.data;
    };
    return RecognizeAction;
}());
exports.RecognizeAction = RecognizeAction;
