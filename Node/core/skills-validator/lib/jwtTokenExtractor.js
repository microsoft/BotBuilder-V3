"use strict";
var __assign = (this && this.__assign) || function () {
    __assign = Object.assign || function(t) {
        for (var s, i = 1, n = arguments.length; i < n; i++) {
            s = arguments[i];
            for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
                t[p] = s[p];
        }
        return t;
    };
    return __assign.apply(this, arguments);
};
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
var jsonwebtoken_1 = require("jsonwebtoken");
var claimsIdentity_1 = require("./claimsIdentity");
var endorsementsValidator_1 = require("./endorsementsValidator");
var openIdMetadata_1 = require("./openIdMetadata");
var JwtTokenExtractor = (function () {
    function JwtTokenExtractor(tokenValidationParameters, metadataUrl, allowedSigningAlgorithms) {
        this.tokenValidationParameters = __assign({}, tokenValidationParameters);
        this.tokenValidationParameters.algorithms = allowedSigningAlgorithms;
        this.openIdMetadata = JwtTokenExtractor.getOrAddOpenIdMetadata(metadataUrl);
    }
    JwtTokenExtractor.getOrAddOpenIdMetadata = function (metadataUrl) {
        var metadata = JwtTokenExtractor.openIdMetadataCache.get(metadataUrl);
        if (!metadata) {
            metadata = new openIdMetadata_1.OpenIdMetadata(metadataUrl);
            JwtTokenExtractor.openIdMetadataCache.set(metadataUrl, metadata);
        }
        return metadata;
    };
    JwtTokenExtractor.prototype.getIdentityFromAuthHeader = function (authorizationHeader, channelId, requiredEndorsements) {
        return __awaiter(this, void 0, void 0, function () {
            var parts;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        if (!authorizationHeader) {
                            return [2, null];
                        }
                        parts = authorizationHeader.split(' ');
                        if (!(parts.length === 2)) return [3, 2];
                        return [4, this.getIdentity(parts[0], parts[1], channelId, requiredEndorsements || [])];
                    case 1: return [2, _a.sent()];
                    case 2: return [2, null];
                }
            });
        });
    };
    JwtTokenExtractor.prototype.getIdentity = function (scheme, parameter, channelId, requiredEndorsements) {
        if (requiredEndorsements === void 0) { requiredEndorsements = []; }
        return __awaiter(this, void 0, void 0, function () {
            var err_1;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        if (scheme !== 'Bearer' || !parameter) {
                            return [2, null];
                        }
                        if (!this.hasAllowedIssuer(parameter)) {
                            return [2, null];
                        }
                        _a.label = 1;
                    case 1:
                        _a.trys.push([1, 3, , 4]);
                        return [4, this.validateToken(parameter, channelId, requiredEndorsements)];
                    case 2: return [2, _a.sent()];
                    case 3:
                        err_1 = _a.sent();
                        console.error('JwtTokenExtractor.getIdentity:err!', err_1);
                        throw err_1;
                    case 4: return [2];
                }
            });
        });
    };
    JwtTokenExtractor.prototype.hasAllowedIssuer = function (jwtToken) {
        var decoded = jsonwebtoken_1.decode(jwtToken, { complete: true });
        var issuer = decoded.payload.iss;
        if (Array.isArray(this.tokenValidationParameters.issuer)) {
            return this.tokenValidationParameters.issuer.indexOf(issuer) !== -1;
        }
        if (typeof this.tokenValidationParameters.issuer === 'string') {
            return this.tokenValidationParameters.issuer === issuer;
        }
        return false;
    };
    JwtTokenExtractor.prototype.validateToken = function (jwtToken, channelId, requiredEndorsements) {
        return __awaiter(this, void 0, void 0, function () {
            var decodedToken, keyId, metadata, decodedPayload_1, endorsements_1, isEndorsed, additionalEndorsementsSatisfied, claims;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        decodedToken = jsonwebtoken_1.decode(jwtToken, { complete: true });
                        keyId = decodedToken.header.kid;
                        return [4, this.openIdMetadata.getKey(keyId)];
                    case 1:
                        metadata = _a.sent();
                        if (!metadata) {
                            throw new Error('Signing Key could not be retrieved.');
                        }
                        try {
                            decodedPayload_1 = jsonwebtoken_1.verify(jwtToken, metadata.key, this.tokenValidationParameters);
                            endorsements_1 = metadata.endorsements;
                            if (Array.isArray(endorsements_1) && endorsements_1.length !== 0) {
                                isEndorsed = endorsementsValidator_1.EndorsementsValidator.validate(channelId, endorsements_1);
                                if (!isEndorsed) {
                                    throw new Error("Could not validate endorsement for key: " + keyId + " with endorsements: " + endorsements_1.join(','));
                                }
                                additionalEndorsementsSatisfied = requiredEndorsements.every(function (endorsement) { return endorsementsValidator_1.EndorsementsValidator.validate(endorsement, endorsements_1); });
                                if (!additionalEndorsementsSatisfied) {
                                    throw new Error("Could not validate additional endorsement for key: " + keyId + " with endorsements: " + requiredEndorsements.join(',') + ". Expected endorsements: " + requiredEndorsements.join(','));
                                }
                            }
                            if (this.tokenValidationParameters.algorithms) {
                                if (this.tokenValidationParameters.algorithms.indexOf(decodedToken.header.alg) === -1) {
                                    throw new Error("\"Token signing algorithm '" + decodedToken.header.alg + "' not in allowed list");
                                }
                            }
                            claims = Object.keys(decodedPayload_1).reduce(function (acc, key) {
                                acc.push({ type: key, value: decodedPayload_1[key] });
                                return acc;
                            }, []);
                            return [2, new claimsIdentity_1.ClaimsIdentity(claims, true)];
                        }
                        catch (err) {
                            console.error("Error finding key for token. Available keys: " + metadata.key);
                            throw err;
                        }
                        return [2];
                }
            });
        });
    };
    JwtTokenExtractor.openIdMetadataCache = new Map();
    return JwtTokenExtractor;
}());
exports.JwtTokenExtractor = JwtTokenExtractor;
