"use strict";
var uuid = require('node-uuid');
var HangupAction = (function () {
    function HangupAction(session) {
        this.session = session;
        this.data = {};
        this.data.action = 'hangup';
        this.data.operationId = uuid.v4();
    }
    HangupAction.prototype.toAction = function () {
        return this.data;
    };
    return HangupAction;
}());
exports.HangupAction = HangupAction;
