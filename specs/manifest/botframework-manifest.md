# Bot Framework -- Manifest

## Abstract

The Bot Framework Manifest format describes a service capable of sending and receiving Bot Framework-compatible messages. The manifest allows configuration, registration, and publishing data about that bot to be authored, stored, and transmitted between services.

## Table of Contents

1. [Introduction](#Introduction)
2. [Basic manifest structure](#Basic-manifest-structure)
3. [Serialization](#Serialization)
4. [Use in APIs](#Use-in-APIs)
5. [Identity fields](#Identity-fields)
6. [Action fields](#Action-fields)
7. [Publishing fields](#Publishing-fields)
8. [Complex types](#Complex-types)
9. [References](#References)

## Introduction

### Overview

The Bot Framework Protocol [[1](#References)] and corresponding Activity schema [[2](#References)] describe a language for sending and receiving conversational messages between humans and automated software. Software that implements the "bot" side of this protocol are frequently registered so they can be contacted by users.

The Bot Framework Manifest format, defined here, describes how to contact this bot, what capabilities it offers, and information about how it should be published. Fields in the manifest appear flat at the root level but are organized within this document according to each goal.

This format is intended to support initial development of the bot, where tools can be used to automate source code templates based on manifest contents; testing of the bot, by providing information about its configuration; and finally, registering the bot so it can be used in production. During the development process, tools such as [msbot](https://github.com/Microsoft/botbuilder-tools/tree/master/MSBot) [[3](#References)] may be used in conjunction with or as a precursor to the manifest.

### Requirements

The key words "MUST", "MUST NOT", "REQUIRED", "SHALL", "SHALL NOT", "SHOULD", "SHOULD NOT", "RECOMMENDED", "MAY", and "OPTIONAL" in this document are to be interpreted as described in [RFC 2119](https://tools.ietf.org/html/rfc2119) [[3](#References)].

An implementation is not compliant if it fails to satisfy one or more of the MUST or REQUIRED level requirements for the protocols it implements. An implementation that satisfies all the MUST or REQUIRED level and all the SHOULD level requirements for its protocols is said to be "unconditionally compliant"; one that satisfies all the MUST level requirements but not all the SHOULD level requirements for its protocols is said to be "conditionally compliant."

### Numbered requirements

Lines beginning with markers of the form `MXXXX` are specific requirements designed to be referenced by number in discussion outside of this document. They do not carry any more or less weight than normative statements made outside of `MXXXX` lines.

`M1000`: Editors of this specification MAY add new `MXXXX` requirements. They SHOULD find numeric `MXXXX` values that preserve the document's flow.

`M1001`: Editors MUST NOT renumber existing `MXXXX` requirements.

`M1002`: Editors MAY delete or revise `MXXXX` requirements. If revised, editors SHOULD retain the existing `MXXXX` value if the topic of the requirement remains largely intact.

`M1003`: Editors SHOULD NOT reuse retired `MXXXX` values. A list of deleted values MAY be maintained at the end of this document.

### Terminology

bot
> Software that sends and receives activities conformant with the Bot Framework Protocol. Bots are also referred to as *skills*, *agents*, and otherwise, although this document uses the term *bot* exclusively.

manifest
> Stored or transmitted description of a bot that conforms to the Manifest format.

reader
> Software reading a manifest.

writer
> Software writing a manifest.

registry
> Software, typically accessed via a portal or API, that accepts manifests for storage and indexing.

field
> A named value within a manifest or nested object.

### Overall organization

The manifest format is a flat list of name/value pairs, some of which are primitive objects, and some of which are complex (nested). The manifest is commonly expressed in the JSON format, but can also be projected into in-memory data structures in languages like C# and JavaScript.

Most fields within the manifest are optional. Developers may wish to use all features of the manifest format to describe their bot, and others will choose to use parts of the format. The format does not put guidelines on the completeness of a manifest, and a manifest that contains only a small number of fields may still be useful for readers.

Even though the manifest is flat, its fields are organized into three categories:
1. What the bot is and where it can be contacted ([identity fields](#Identity-fields)])
2. What the bot can do ([action fields](#Action-fields))
3. Where the bot is published ([publishing fields](#Publishing-fields))
The ordering of fields within this document is intended to place similar concepts in the same place in the document.

The document is intended to be extensible except where specifically noted. To preserve interoperability, writers are encouraged to retain the meaning of existing fields so that a generic reader can make sense of their contents even when extensions are added.

## Basic manifest structure

This section defines the requirements for the basic structure of the manifest. JSON is used as the common interchange format and defines limitations for field contents and uniqueness. (For example, XML-style attributes on field are not allowed.)

`M2000`: Manifest contents MUST be serializable to the JSON format, including adherence to e.g. field uniqueness constraints.

`M2002`: Readers MAY allow improperly-cased field names, although this is not required. If readers allow any improperly-cased field names, they MUST allow all fields to be improperly cased.

`M2003`: Unless otherwise noted, writers SHOULD NOT include empty string values for string fields.

`M2004`: Unless otherwise noted, writers SHOULD NOT include empty arrays or complex objects without child fields.

`M2005`: Unless otherwise noted, emitters MAY include additional fields within the manifest or any nested complex objects. Readers MUST accept fields they do not understand.

`M2006`: Writers SHOULD preserve the order of fields as described in this specification. Readers MUST allow fields to be expressed in other orders.

## Serialization

Manifests are valid in the ".man" file format described in this section.

### .man file format

`M2100`: Valid .man files MUST be a serialized JSON enitity.

`M2101`: Writers SHOULD use UTF-8 encoding for all .man files. Writers SHOULD NOT include byte-order marks (BOM) in .man files. Readers MAY reject .man files that include BOM or encoding other than UTF-8.

`M2102`: For legibilty, writers SHOULD pretty-format JSON so it includes newlines and whitespace.

## Use in APIs

Manifest contents are commonly exchanged via APIs and those APIs may expose raw file upload or more fine-grained REST APIs that include manifest fields. In the latter case, APIs that faithfully map manifest content and meaning will have the best interoperability with the Bot Framework ecosystem.

### Registries

The most common use for manifest APIs is as a way to accept manifests as part of a registry.

`M3000`: Registries SHOULD allow raw manifest upload when possible.

`M3001`: Registries MAY map manifest content and meaning to REST APIs.

`M3002`: Registries SHOULD allow raw manifests to be downloaded, even if upload is not supported.

`M3003`: Unless otherwise noted, registries SHOULD accept the entire manifest or reject it entirely. Registries SHOULD NOT omit fields that are understood but fail to meet syntax or policy requirements. Registries MAY ignore fields they do not understand.

Frequently, registries accept manifests in order to list bots for a directory or repository. In these cases, the assignment of IDs is sometimes allowed only by the registry itself.

`M3010`: Registries MAY reject manifests with empty or missing IDs.

`M3011`: Registries MAY allow manifests with empty or missing IDs. If IDs are assigned, the registry SHOULD communicate these IDs back to the submitter.

## Identity fields

The fields in this description describe what the bot is and where to contact it. Typically these fields are used to assign stable identifiers (for later operations, like updates and deletes) and contact the bot once a channel or orchestrator has decided to route traffic there. Fields in this section appear at the root of the manifest alongside [action fields](#Action-fields) and [publishing fields](#Publishing-fields).

### Id

The `id` field represents the unique ID of the bot within the system in which it is registered (e.g. the Bot Framework development portal). The value of the `id` field is of type string.

### MsaAppId

The `msaAppId` field contains the ID of the MSA/AAD app that represents the bot for service-to-service auth. The value of the `msaAppId` field is of type string.

### Endpoint

The `endpoint` field contains the endpoint URL of the bot compatible with the Bot Framework Protocol [[1](#References)]. This endpoint definition has a fixed protocol (the Bot Framework Protocol) that can be used with many bindings, the most common being HTTP/S. For simplicity, the HTTP/S binding is fixed to the binding defined in the Bot Framework Protocol, although other URL schemes may be used to define different bindings as long as the core protocol semantics are retained.

The value of the `endpoint` field is a URL [[5](#References)] within a string.

`M4030`: Writers MUST NOT overload the `endpoint` field to contain endpoints with alternative protocols.

`M4031`: Writers MUST NOT use HTTP- or HTTPS-scheme URLs to represent alternative bindings, even if those bindings operate over HTTP/S.

`M4032`: Writers MAY use non-HTTP/S-scheme URLs to represent alternative bindings.

### Icon URL

The `iconUrl` field contains the URL of the bot's icon. The value of the `iconUrl` is a URI [[5](#References)] within a string.

Registries typically expect icon URLs with HTTP or HTTPS schemes, and some registries require HTTPS. For this reason, HTTPS is the most interoperable way to deliver icons. However, alternative delivery mechanisms, such as local files or Data URIs (as defined in [RFC 2397](https://tools.ietf.org/html/rfc2397) [[7](#References)]) may be supported by some regsitries.

`M4100`: Registries SHOULD accept `iconUrl` values of HTTPS scheme.

### Authentication connections

Some registars accept definitions for authentication providers that bots can use at runtime to collect sign-in and access consent from users. The configuration for this information is stored in the `authenticationConnections` field. The `authenticationConnections` field is an array of type [`authenticationConnection`](#Authentication-connection)

## Action fields

There is only one field in the actions section: [`actions`](#Actions). This field describes the capabilities that the bot advertises.

### Actions

The `actions` field contains a list of actions the bot advertises it can accept. The `actions` field is an array of type ['action'](#Action).

Typically actions are used to describe functions central to a bot's purpose, but not ancillary functionality necessary to complete the bot's user experience. For example, a bot for filing bugs could advertise an action for filing a bug, but not advertise an action for surfacing help requests.

Actions are sometimes defined by the bot, for example when the functionality is specific to the bot, and in other cases is defined globally so more than one bot can register for the same action. The granularity and details of these actions are up to whoever defines them.

In cases where the action is defined by the bot, the definition may be included inline within the action registration. In cases where the bot is only registering for a predefined action, the action registration is unlikely to include the definition.

An action advertisement is not a guarantee that the bot can successfully service a request that meets the syntactical requirements for the action. Instead, it merely indicates that the bot *might* be able to handle requests for that action. Correspondingly, a bot advertising an action is not guaranteed to receive all fields and all entities defined for an action.

### Publishing fields

The publishing section contains information about how the bot is published into a registry, typically for discovery by users. The name, structure, and meaning of the fields are established by the registry.

`M4800`: Writers SHOULD only include publishing fields applicable to the registry where the manifest is intended to be consumed. If the manifest is not intended for consumption within a registry, writers SHOULD NOT include any publishing fields.

## Complex types

### Action

The `action` type describes a single action registration, and optionally the action's definition. More details on action registrations and definitions can be found in the ['actions'](#Actions) topic.

#### ID

The `id` field contains the unique ID of the action within the system in which it is registered (e.g. the Bot Framework development portal). The value of the `id` field is of type string.

#### Definition

The `definition` field contains the definition for an action. In many cases, the definition is omitted. The type of the `definition` field is of type [`actionDefinition`](#Action-definition).

`M5020`: Readers who do not allow modification of existing action definitions SHOULD ignore `definition` fields for actions that cannot be updated.

`M5021`: Readers SHOULD reject definitions if both the action and the definition contain `id` fields and those fields are not identical.

### Action definition

The `actionDefinition` type declares the shape and meaning of an action. Actions may be triggered by utterance, direct binding (via a button), physical gesture, regex, and many other schemes. Some of these triggering techniques are expressed directly within the action definition: for example, the [`utterances`](#Utterances) field contains examples that could be used to train an NLP dispatching system. Some triggering systems are outside the scope of this specification, but may be used by readers and writers of the Action.

#### ID

The `id` field contains the unique ID of the action within the system in which it is registered. The `id` field is redundant with the ID field in the [`action`](#Action) type.

`M5110`: Writers SHOULD omit the `id` field within the `actionDefinition` type if it is contained within an `action` with a corresponding `id`.

#### Description

The `description` field contains a textual description of the action. The value of the `description` field is of type string.

#### Entities

The `entities` field contains a list of entity definitions for the action. The value of the `entities` field is an array of type [`entityDefinition`](#Entity-definition).

`M5130`: Readers and writers SHOULD preserve the order of entity definitions.

#### Triggers

The `triggers` field contains triggers that orchestrators may use to activate the action. The value of the `triggers` field is of type [`triggerSet`](#Trigger-set).

### Entity definition

The `entityDefinition` type declares the shape of entities to be submitted within the action.

#### Name

The `name` field uniquely defines the entity within this action. The value of the `name` field is of type string.

`M2510`: Writers MUST include a unique `name` for each entity definition within an action definition.

`M5211`: Readers SHOULD use ordinal comparison to establish equivalency of the value of the `name` field within an action.

`M5212`: Writers MUST NOT emit two entities with the same name.

#### Type

The `type` field defines the entity's type. The value of the `type` field is a URI [[5](#References)] within a string.

`M5220`: Writers MUST include a `type` for each entity definition.

`M5221`: Readers SHOULD use ordinal comparison to establish equivalency of the value of the `type` field. Readers SHOULD NOT use URI ladder comparisons.

### Trigger set

The `triggerSet` type describes a set of conditions for activation, typically in the context of an [action definition](#Action-definition). The conditions for activation are the union (i.e., the logical "OR") of all available triggers.

#### Utterances

The `utterances` field contains a list of text utterances which may trigger this action. The value of the `utterances` field is an array of type [`utteranceDefinition`](#Utterance-definition).

### Utterance definition

An utterance contains raw data that can be used to identify when, for example, a user's request should trigger this action. The implementation of the system that learns, extracts, and matches text against these utterances is expected to vary across systems.

#### Text

The `text` field contains the raw text associated with this utterance. The value of the `text` field is of type string.

#### Locale

The `locale` field communicates the language code of the [`text`](#Text) field. The value of the `locale` field is an [ISO 639](https://www.iso.org/iso-639-language-codes.html) [[6](#References)] code within a string.

`M5320`: Readers SHOULD treat missing and unknown values of the `locale` field as unknown.

`M5321`: Readers SHOULD NOT reject activities with unknown locale.

#### Entities

The `entities` field contains a list of entities present in the source [`text`](#Text). The value of the `entities` field is an array of type [`entityReference`](#Entity-reference).

`M5330`: Writers SHOULD NOT include entity references that do not match [`name`](#Name) field values defined for the action.

`M5331`: Readers SHOULD ignore entity references that do not match [`name`](#Name) field values defined for the action.

### Entity reference

The `entityReference` type identifies a entity embedded within text.

#### Name

The `name` field identifies the referenced entity. The value of the `name` field is of type string.

`M5410`: Readers SHOULD use ordinal comparison to establish equivalency of the value of the `name` field within an action.

#### Start position

The `startPosition` field identifies the position within the [`text`](#Text) field where the first character of the entity is found. The value of the `startPosition` field is of type number.

#### End position

The `endPosition` field identifies the position within the [`text`](#Text) field where the last character of the entity is found. The value of the `endPosition` field is of type number.

`M5430`: Writers MUST include `name`, `startPosition`, and `endPosition` in all `entityReference` objects.

`M5431`: Writers MUST only use non-negative integer numbers for the value of `startPosition` and `endPosition`.

`M5432`: Writers MUST include `startPosition` and `endPosition` values between 0 and the length of the [`text`](#Text) field.

`M5433`: Writers MUST NOT include `endPosition` values that are less than or equal to `startPosition` values.

`M5434`: Readers SHOULD reject any `entityReference` objects that do not meet the above criteria.

### Authentication connection

An authentication connection represents a sign-in service, and contains parameters describing how to use this service to request user sign-in and consent.

#### Authentication connection ID

The `id` field contains the unique internal identifier for this setting. The value of the `id` field is of type string.

`M5510`: Two `id` values are equivalent only if they are ordinally identical.

#### Authentication connection name

The `name` field contains a name used to identify the authentication connection when invoked 

#### Authentication connection service provider ID

The `serviceProviderId` field identifies the authentication service providing sign-in functionality. The value of the `serviceProviderId` is of type string and the valid values (and their meanings) are definde by the registry accepting the manifest.

#### Authentication connection client ID

The `clientId` field contains the identifier sent to the authentication service when requesting sign-in. The value of the `clientId` field is of type string.

#### Authnetication connection client secret

The `clientSecret` field contains the client secret sent to the authentication service when requesting sign-in. The value of the `clientSecret` field is of type string.

#### Authentication connection scope

The `scopes` field contains the scopes string sent to the authentication service when requesting sign-in. If the authentication connection specifies multiple scopes, they are supplied in this field in whatever format the authentication service is expecting. The value of the `scopes` field is of type string.

#### Authentication connection properties

The `properties` field contains additional properties to be supplied to the service provider identified by [`serviceProviderId`](#Authentication-connection-service-provider-ID). The value of the `properties` field is a complex type defined by the service provider.

`M5560`: Registries and service providers SHOULD use only strings for authentication property keys and values.

### References

1. [Bot Framework Protocol](../botframework-protocol/botframework-protocol.md)
2. [Bot Framework Activity](../botframework-activity/botframework-activity.md)
3. [MSBot](https://github.com/Microsoft/botbuilder-tools/tree/master/MSBot)
4. [RFC 2119](https://tools.ietf.org/html/rfc2119)
5. [RFC 3987](https://tools.ietf.org/html/rfc3987)
6. [ISO 639](https://www.iso.org/iso-639-language-codes.html)
7. [RFC 2397](https://tools.ietf.org/html/rfc2397)

# Appendix I - Changes

## 2018-08-19 - dandris@microsoft.com
* Rearranged triggers into own section

## 2018-08-13 - dandris@microsoft.com

* Add omitted text in overview descriptions
* Revise `M5431` to allow zero

## 2018-07-31 - dandris@microsoft.com

* Move identity/actions/publishing into distinct sections

## 2018-07-29 - dandris@microsoft.com

* Added icons back into format
* Added authentication connections

## 2018-07-25 - dandris@microsoft.com

* Initial draft
