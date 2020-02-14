"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
Object.defineProperty(exports, "__esModule", { value: true });
var url = require("url");
var adal = require("adal-node");
var authenticationConstants_1 = require("./authenticationConstants");
var AppCredentials = (function () {
    function AppCredentials(appId, oAuthScope) {
        if (oAuthScope === void 0) { oAuthScope = authenticationConstants_1.AuthenticationConstants.ToBotFromChannelTokenIssuer; }
        this.refreshingToken = null;
        this.appId = appId;
        var tenant = authenticationConstants_1.AuthenticationConstants.DefaultChannelAuthTenant;
        this.oAuthEndpoint = authenticationConstants_1.AuthenticationConstants.ToChannelFromBotLoginUrlPrefix + tenant;
        this.oAuthScope = oAuthScope;
        this.authenticationContext = new adal.AuthenticationContext(this.oAuthEndpoint, true, undefined, '1.5');
    }
    Object.defineProperty(AppCredentials.prototype, "oAuthScope", {
        get: function () {
            return this._oAuthScope;
        },
        set: function (value) {
            this._oAuthScope = value;
            this.tokenCacheKey = "" + this.appId + this.oAuthScope + "-cache";
        },
        enumerable: true,
        configurable: true
    });
    AppCredentials.trustServiceUrl = function (serviceUrl, expiration) {
        if (!expiration) {
            expiration = new Date(Date.now() + 86400000);
        }
        var uri = url.parse(serviceUrl);
        if (uri.host) {
            AppCredentials.trustedHostNames.set(uri.host, expiration);
        }
    };
    AppCredentials.isTrustedServiceUrl = function (serviceUrl) {
        try {
            var uri = url.parse(serviceUrl);
            if (uri.host) {
                return AppCredentials.isTrustedUrl(uri.host);
            }
        }
        catch (e) {
            console.error('Error in isTrustedServiceUrl', e);
        }
        return false;
    };
    AppCredentials.isTrustedUrl = function (uri) {
        var expiration = AppCredentials.trustedHostNames.get(uri);
        if (expiration) {
            return expiration.getTime() > (Date.now() - 300000);
        }
        return false;
    };
    AppCredentials.prototype.signRequest = function (webResource, authorizationScheme) {
        if (authorizationScheme === void 0) { authorizationScheme = 'Bearer'; }
        return __awaiter(this, void 0, void 0, function () {
            var token;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        if (!this.shouldSetToken(webResource)) return [3, 2];
                        return [4, this.getToken()];
                    case 1:
                        token = _a.sent();
                        webResource.headers = { 'authorization': authorizationScheme + " " + token };
                        _a.label = 2;
                    case 2: return [2, Promise.resolve(webResource)];
                }
            });
        });
    };
    AppCredentials.prototype.getToken = function (forceRefresh) {
        if (forceRefresh === void 0) { forceRefresh = false; }
        return __awaiter(this, void 0, void 0, function () {
            var oAuthToken, res;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        if (!forceRefresh) {
                            oAuthToken = AppCredentials.cache.get(this.tokenCacheKey);
                            if (oAuthToken) {
                                if (oAuthToken.expirationTime > Date.now()) {
                                    return [2, oAuthToken.accessToken];
                                }
                            }
                        }
                        return [4, this.refreshToken()];
                    case 1:
                        res = _a.sent();
                        this.refreshingToken = null;
                        if (res && res.accessToken) {
                            res.expirationTime = Date.now() + (res.expiresIn * 1000) - 300000;
                            AppCredentials.cache.set(this.tokenCacheKey, res);
                            return [2, res.accessToken];
                        }
                        else {
                            throw new Error('Authentication: No response or error received from ADAL.');
                        }
                        return [2];
                }
            });
        });
    };
    AppCredentials.prototype.shouldSetToken = function (webResource) {
        return AppCredentials.isTrustedServiceUrl(webResource.url);
    };
    AppCredentials.trustedHostNames = new Map([
        ['state.botframework.com', new Date(8640000000000000)],
        ['api.botframework.com', new Date(8640000000000000)],
        ['token.botframework.com', new Date(8640000000000000)],
        ['state.botframework.azure.us', new Date(8640000000000000)],
        ['api.botframework.azure.us', new Date(8640000000000000)],
        ['token.botframework.azure.us', new Date(8640000000000000)],
    ]);
    AppCredentials.cache = new Map();
    return AppCredentials;
}());
exports.AppCredentials = AppCredentials;
