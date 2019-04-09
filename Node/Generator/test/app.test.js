'use strict';
const path = require('path');
const rimraf = require('rimraf');
const assert = require('yeoman-assert');
const helpers = require('yeoman-test');

const botName = 'sample';
const description = 'sample';

describe('Creates a bot based on Javascript', () => {
  // Setup
  beforeAll(() => {
    // The object returned acts like a promise, so return it to wait until the process is done
    return helpers.run(path.join(__dirname, '../generators/app'))
      .inDir(path.join(__dirname, 'tmp'))
      .withPrompts({botName: botName, description: description, language: 'JavaScript'});  // Mock the prompt answers
  });

  // Tear down
  afterAll(() => {
    // Delete 'tmp' directory
    rimraf.sync(path.join(__dirname, 'tmp'));
  });

  it('generates a bot app with all of the required files', () => {
    // Assertions
    assert.file([
      path.join(__dirname, `tmp/${botName}/.env`),
      path.join(__dirname, `tmp/${botName}/.gitignore`),
      path.join(__dirname, `tmp/${botName}/app.js`),
      path.join(__dirname, `tmp/${botName}/bot.js`),
      path.join(__dirname, `tmp/${botName}/package.json`),
      path.join(__dirname, `tmp/${botName}/README.md`)
    ]);
  });
});

describe('Creates a bot based on Typescript', () => {
  // Setup
  beforeAll(() => {
    // The object returned acts like a promise, so return it to wait until the process is done
    return helpers.run(path.join(__dirname, '../generators/app'))
      .inDir(path.join(__dirname, 'tmp'))
      .withPrompts({botName: botName, description: description, language: 'TypeScript'});  // Mock the prompt answers
  });

  // Tear down
  afterAll(() => {
    // Delete 'tmp' directory
    rimraf.sync(path.join(__dirname, 'tmp'));
  });

  it('generates a bot app with all of the required files', () => {
    // Assertions
    assert.file([
      path.join(__dirname, `tmp/${botName}/.env`),
      path.join(__dirname, `tmp/${botName}/.gitignore`),
      path.join(__dirname, `tmp/${botName}/app.ts`),
      path.join(__dirname, `tmp/${botName}/bot.ts`),
      path.join(__dirname, `tmp/${botName}/package.json`),
      path.join(__dirname, `tmp/${botName}/README.md`)
    ]);
  });
});

