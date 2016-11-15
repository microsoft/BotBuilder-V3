---
layout: page
title: Setting up continuous integration
permalink: /en-us/azure-bot-service/manage/setting-up-continuous-integration/
weight: 13010
parent1: Azure Bot Service
parent2: Manage
---

If the online code editor is not enough for your development needs, you can setup continuous integration, download the code, use your favorite IDE, and then deploy directly to your Azure Bot. Follow these easy steps to get started.

* TOC
{:toc}

<div class="docs-text-note"><strong>Note:</strong> When you setup continuous integration, the bot code editor within the Azure portal will be disabled. To re-enable it you will need to disconnect your deployment source.</div>

<div class="docs-text-note"><strong>Note:</strong> This document highlights the specific continuous integration features in Azure Bots, please refer to this <a href="https://azure.microsoft.com/en-us/documentation/articles/app-service-continuous-deployment/" target="_blank">page</a> to get more detailed information about continuous integration in Azure App Services.</div>

## 1. Create an empty repository in your favorite source control system

The first step is to create an empty repository. At the time of this writing, Azure supports the following sources 

[![Source control system](/en-us/images/azure-bots/continuous-integration-sourcecontrolsystem.png)](/en-us/images/azure-bots/continuous-integration-sourcecontrolsystem.png)

<div class="imagecaption"><span>Azure deployment sources</span></div>


## 2. Download the bot code

1. Download the bot code zip file from the Settings tab of your Azure Bot.

    [![Download the bot zip file](/en-us/images/azure-bots/continuous-integration-download.png)](/en-us/images/azure-bots/continuous-integration-download.png)

    <div class="imagecaption"><span>Downloading your bot code</span></div>

2. Unzip the bot code into your local drive where you are planning to sync your deployment source.

## 3. Choose the deployment source and connect your repository

1. Click on the Settings tab within your Azure Bot, and expand the continuous integration section
2. Then click on "Setup integration source"

    [![Setup integration source](/en-us/images/azure-bots/continuous-integration-setupclick.png)](/en-us/images/azure-bots/continuous-integration-setupclick.png)

    <div class="imagecaption"><span>Accessing the continuous deployment Azure blade</span></div>

3. Click Setup, select your favorite deployment source, and follow the steps to connect it. Make sue you select the repository you created at the beginning

    [![Setup integration source](/en-us/images/azure-bots/continuous-integration-sources.png)](/en-us/images/azure-bots/continuous-integration-sources.png)

    <div class="imagecaption"><span>Select your favorite deployment source</span></div>

## Disconnecting your deployment source

If, for any reason, you need to disconnect your deployment source from your bot, simply open the Settings tab, expand the continuous integration section, click on "Setup integration source", and finally click on disconnect in the resulting blade.

[![Disconnect your deployment source](/en-us/images/azure-bots/continuous-integration-disconnect.png)](/en-us/images/azure-bots/continuous-integration-disconnect.png)

<div class="imagecaption"><span>Disconnecting your deployment source</span></div>

## Next steps

* Learn how to [debug your local code](/en-us/azure-bot-service/manage/debug/)