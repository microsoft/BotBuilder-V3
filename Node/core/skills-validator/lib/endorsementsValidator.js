"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var EndorsementsValidator = (function () {
    function EndorsementsValidator() {
    }
    EndorsementsValidator.validate = function (channelId, endorsements) {
        if (channelId === null || channelId.trim() === '') {
            return true;
        }
        if (endorsements === null) {
            throw new Error('endorsements required');
        }
        return endorsements.some(function (value) { return value === channelId; });
    };
    return EndorsementsValidator;
}());
exports.EndorsementsValidator = EndorsementsValidator;
