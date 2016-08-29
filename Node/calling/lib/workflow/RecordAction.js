"use strict";
var uuid = require('node-uuid');
var utils = require('../utils');
exports.RecordingFormat = {
    wma: 'wma',
    wav: 'wav',
    mp3: 'mp3'
};
exports.RecordingCompletionReason = {
    unknown: 'unknown',
    initialSilenceTimeout: 'initialSilenceTimeout',
    maxRecordingTimeout: 'maxRecordingTimeout',
    completedSilenceDetected: 'completedSilenceDetected',
    completedStopToneDetected: 'completedStopToneDetected',
    callTerminated: 'callTerminated',
    temporarySystemFailure: 'temporarySystemFailure'
};
var RecordAction = (function () {
    function RecordAction(session) {
        this.session = session;
        this.data = session ? utils.clone(session.recordDefaults || {}) : {};
        this.data.action = 'record';
        this.data.operationId = uuid.v4();
    }
    RecordAction.prototype.playPrompt = function (action) {
        if (action) {
            this.data.playPrompt = action.toAction ? action.toAction() : action;
        }
        return this;
    };
    RecordAction.prototype.maxDurationInSeconds = function (time) {
        if (time) {
            this.data.maxDurationInSeconds = time;
        }
        return this;
    };
    RecordAction.prototype.initialSilenceTimeoutInSeconds = function (time) {
        if (time) {
            this.data.initialSilenceTimeoutInSeconds = time;
        }
        return this;
    };
    RecordAction.prototype.maxSilenceTimeoutInSeconds = function (time) {
        if (time) {
            this.data.maxSilenceTimeoutInSeconds = time;
        }
        return this;
    };
    RecordAction.prototype.recordingFormat = function (fmt) {
        if (fmt) {
            this.data.recordingFormat = fmt;
        }
        return this;
    };
    RecordAction.prototype.playBeep = function (flag) {
        this.data.playBeep = flag;
        return this;
    };
    RecordAction.prototype.stopTones = function (dtmf) {
        if (dtmf) {
            this.data.stopTones = dtmf;
        }
        return this;
    };
    RecordAction.prototype.toAction = function () {
        return this.data;
    };
    return RecordAction;
}());
exports.RecordAction = RecordAction;
