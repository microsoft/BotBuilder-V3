"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var SimpleCredentialProvider = (function () {
    function SimpleCredentialProvider(appId, appPassword) {
        this.appId = appId;
        this.appPassword = appPassword;
    }
    SimpleCredentialProvider.prototype.isValidAppId = function (appId) {
        return Promise.resolve(this.appId === appId);
    };
    SimpleCredentialProvider.prototype.getAppPassword = function (appId) {
        return Promise.resolve((this.appId === appId) ? this.appPassword : null);
    };
    SimpleCredentialProvider.prototype.isAuthenticationDisabled = function () {
        return Promise.resolve(!this.appId);
    };
    return SimpleCredentialProvider;
}());
exports.SimpleCredentialProvider = SimpleCredentialProvider;
