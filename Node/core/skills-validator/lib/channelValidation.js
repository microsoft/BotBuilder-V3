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
var authenticationConstants_1 = require("./authenticationConstants");
var authenticationConfiguration_1 = require("./authenticationConfiguration");
var jwtTokenExtractor_1 = require("./jwtTokenExtractor");
var ChannelValidation;
(function (ChannelValidation) {
    ChannelValidation.ToBotFromChannelTokenValidationParameters = {
        issuer: [authenticationConstants_1.AuthenticationConstants.ToBotFromChannelTokenIssuer],
        audience: undefined,
        clockTolerance: 5 * 60,
        ignoreExpiration: false
    };
    function authenticateChannelTokenWithServiceUrl(authHeader, credentials, serviceUrl, channelId, authConfig) {
        if (authConfig === void 0) { authConfig = new authenticationConfiguration_1.AuthenticationConfiguration(); }
        return __awaiter(this, void 0, void 0, function () {
            var identity, serviceUrlClaim;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4, authenticateChannelToken(authHeader, credentials, channelId, authConfig)];
                    case 1:
                        identity = _a.sent();
                        serviceUrlClaim = identity.getClaimValue(authenticationConstants_1.AuthenticationConstants.ServiceUrlClaim);
                        if (serviceUrlClaim !== serviceUrl) {
                            throw new Error('Unauthorized. ServiceUrl claim do not match.');
                        }
                        return [2, identity];
                }
            });
        });
    }
    ChannelValidation.authenticateChannelTokenWithServiceUrl = authenticateChannelTokenWithServiceUrl;
    function authenticateChannelToken(authHeader, credentials, channelId, authConfig) {
        if (authConfig === void 0) { authConfig = new authenticationConfiguration_1.AuthenticationConfiguration(); }
        return __awaiter(this, void 0, void 0, function () {
            var tokenExtractor, identity;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        tokenExtractor = new jwtTokenExtractor_1.JwtTokenExtractor(ChannelValidation.ToBotFromChannelTokenValidationParameters, ChannelValidation.OpenIdMetadataEndpoint ? ChannelValidation.OpenIdMetadataEndpoint : authenticationConstants_1.AuthenticationConstants.ToBotFromChannelOpenIdMetadataUrl, authenticationConstants_1.AuthenticationConstants.AllowedSigningAlgorithms);
                        return [4, tokenExtractor.getIdentityFromAuthHeader(authHeader, channelId, authConfig.requiredEndorsements)];
                    case 1:
                        identity = _a.sent();
                        return [4, validateIdentity(identity, credentials)];
                    case 2: return [2, _a.sent()];
                }
            });
        });
    }
    ChannelValidation.authenticateChannelToken = authenticateChannelToken;
    function validateIdentity(identity, credentials) {
        return __awaiter(this, void 0, void 0, function () {
            var audClaim;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        if (!identity || !identity.isAuthenticated) {
                            throw new Error('Unauthorized. Is not authenticated');
                        }
                        if (identity.getClaimValue(authenticationConstants_1.AuthenticationConstants.IssuerClaim) !== authenticationConstants_1.AuthenticationConstants.ToBotFromChannelTokenIssuer) {
                            throw new Error('Unauthorized. Issuer Claim MUST be present.');
                        }
                        audClaim = identity.getClaimValue(authenticationConstants_1.AuthenticationConstants.AudienceClaim);
                        return [4, credentials.isValidAppId(audClaim || '')];
                    case 1:
                        if (!(_a.sent())) {
                            throw new Error("Unauthorized. Invalid AppId passed on token: " + audClaim);
                        }
                        return [2, identity];
                }
            });
        });
    }
    ChannelValidation.validateIdentity = validateIdentity;
})(ChannelValidation = exports.ChannelValidation || (exports.ChannelValidation = {}));
