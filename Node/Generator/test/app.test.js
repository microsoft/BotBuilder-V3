'use strict';
const path = require('path');
const rimraf = require('rimraf');
const assert = require('yeoman-assert');
const helpers = require('yeoman-test');

const botName = 'sample';
const description = 'sample';

describe('Creates a bot based on Javascript', () => {
  const tmpDir = 'tmp_js';
  // Setup
  beforeAll(() => {
    // The object returned acts like a promise, so return it to wait until the process is done
    return helpers.run(path.join(__dirname, '../generators/app'))
      .inDir(path.join(__dirname, tmpDir))
      .withPrompts({botName: botName, description: description, language: 'JavaScript'});  // Mock the prompt answers
  });

  // Tear down
  afterAll(() => {
    // Delete 'tmp' directory
    rimraf.sync(path.join(__dirname, tmpDir));
  });

  it('generates a bot app with all of the required files', () => {
    // Assertions
    assert.file([
      path.join(__dirname, `${tmpDir}/${botName}/.env`),
      path.join(__dirname, `${tmpDir}/${botName}/.gitignore`),
      path.join(__dirname, `${tmpDir}/${botName}/app.js`),
      path.join(__dirname, `${tmpDir}/${botName}/bot.js`),
      path.join(__dirname, `${tmpDir}/${botName}/package.json`),
      path.join(__dirname, `${tmpDir}/${botName}/README.md`)
    ]);
  });

  it('App file contains code that creates a server', () => {
    assert.fileContent(path.join(__dirname, `${tmpDir}/${botName}/app.js`), 'restify.createServer');
  });

  it('Bot js file calls the universalBot constructor', () => {
    assert.fileContent(path.join(__dirname, `${tmpDir}/${botName}/bot.js`), 'const bot = new builder.UniversalBot');
  });

});

describe('Creates a bot based on Typescript', () => {
  const tmpDir = 'tmp_ts';
  // Setup
  beforeAll(() => {
    // The object returned acts like a promise, so return it to wait until the process is done
    return helpers.run(path.join(__dirname, '../generators/app'))
      .inDir(path.join(__dirname, tmpDir))
      .withPrompts({botName: botName, description: description, language: 'TypeScript'});  // Mock the prompt answers
  });

  // Tear down
  afterAll(() => {
    // Delete 'tmp' directory
    rimraf.sync(path.join(__dirname, tmpDir));
  });

  it('generates a bot app with all of the required files', () => {
    // Assertions
    assert.file([
      path.join(__dirname, `${tmpDir}/${botName}/.env`),
      path.join(__dirname, `${tmpDir}/${botName}/.gitignore`),
      path.join(__dirname, `${tmpDir}/${botName}/app.ts`),
      path.join(__dirname, `${tmpDir}/${botName}/bot.ts`),
      path.join(__dirname, `${tmpDir}/${botName}/package.json`),
      path.join(__dirname, `${tmpDir}/${botName}/README.md`)
    ]);
  });

  it('App file contains the correct class definition', () => {
    assert.fileContent(path.join(__dirname, `${tmpDir}/${botName}/app.ts`), 'class App');
  });

  it('Bot ts file calls the universalBot constructor', () => {
    assert.fileContent(path.join(__dirname, `${tmpDir}/${botName}/bot.ts`), 'const bot = new builder.UniversalBot');
  });

});