# SocialAlloy
This repository contains a clone of the Episerver Alloy sample application, which has been extended to demonstrate the features of Episerver Social. The goals for this project are twofold:

* To provide a simple application demonstrating Episerver Social features and capabilities
* To provide developers looking to get started with Episerver Social with a helpful point of reference

## Release Notes
* Version 2.0.0
 * This release introduces a "Community" page type, which applies the various Social blocks to demonstrate the creation of community pages for Alloy resellers.
 * Adds a demonstration implementation of a "Like" button using Episerver Social's Ratings feature.
 * Adds a demonstration implementation of IContent extension methods for publishing and retrieving comments.
 * (It is recommended to setup a fresh database for your site when getting started with release. See: Setup)
* Version 1.0.0
 * This release introduces several block types, which demonstrate simple implementations of various Episerver Social features.

## Getting Started
An Episerver Social account is required to run this application. If you do not have an Episerver Social account, please contact your Episerver account manager.

### Setup
* Run the setup script with Powershell: `.\src\Scripts\Setup.ps1`
 * This script will copy the site's database and assets into the appropriate locations.
* Open the SocialAlloy.sln in Visual Studio
* Open the web.config and update the `episerver.social` configuration section with your account information.
 * For more information on how to configure Episerver Social, [please visit our Getting Connected guide](http://world.episerver.com/documentation/developer-guides/social/social_platform-overview/Installing-Episerver-Social/#GettingConnected).

### Running the Application
* Build the application in Visual Studio 2015
 * This operation will restore the necessary NuGet packages dependencies.
* Launch the application from Visual Studio 2015
 * With debugging (F5)
 * Without debugging (CTRL+F5)

## What's Inside?
### Blocks
Many of the Social features demonstrated in this application are implemented using Episerver blocks.  Episerver blocks are available for the following Episerver Social features:

#### Comments
The **CommentBlock** allows visitors to contribute comments to the page on which it resides. (A visitor may contribute comments anonymously or as a logged in user.) The block will display the most recent comments that have been contributed for that page. The maximum number of items displayed by the block are configurable via the CommentDisplayMax property of the block (default = 20).

If the **CommentBlock** is configured to send activities (which is the default behavior), it will publish an activity to the Social Activity Streams system when a comment is contributed to the page. A record of this activity will appear in the activity feed of user's that have subscribed to the page. (See "Activity Streams" below for more information.)

#### Ratings
The **RatingBlock** allows a logged in user to rate the page on which it resides. (A user may only rate the page once.) The block also displays the accumulated rating statistics for the page. 

If the **RatingBlock** is configured to send activities (which is the default behavior), it will publish an activity to the Social Activity Streams system when a rating is submitted by the logged in user. A record of this activity will appear in the activity feed of user's that have subscribed to the page. (See "Activity Streams" below for more information.)

#### Groups
The **GroupCreationBlock** allows a user to create a new group. A group creator can optionally require that new members are moderated before being allowed to join the group.

The **GroupAdmissionBlock** allows users to require membership into a specific group. The block is configured by specifying the name of the group to which it applies. If the associated group is not configured for moderation, a user submitting a request will be added to the group. If the associated group has been configured for moderation, membership requests will be entered into a workflow for review. (To view requests under moderation, see "Moderation UI" below for more information.)

The **MembershipDisplayBlock** allows users to see the members that have joined a group. The block is configured by specifying the name of the group to which it applies. If the associated group has been configured for membership moderation only approved member requests will be displayed in the block. (To view requests that are pending approval, see "Moderation UI" below for more information.)

The **Moderation UI** allows a user to moderate requests to join a group. The page can be found by navigating to the following page within the site: http://*host*:*port*/Moderation. From this page, a user can select a group, which has been configured for moderation, and view the membership requests that have been submitted for it. The user is presented with actions that allow them to move that request through the workflow.  If a request is approved, the requesting user will be added as a member of the group.

#### Activity Streams

The **SubscriptionBlock** allows a logged in user to subscribe to and unsubscribe from the page on which it resides. A user will accumulate a feed of activities from the pages to which they are subscribed. (See "FeedBlock" below for information on displaying a feed.) When a user unsubscribes from a page, activities generated by that page will no longer be added to the user's feed.

The **FeedBlock** displays a record of activities occurring on pages to which the currently logged in user is subscribed. The block will display feed items representing the most recent activities that have occurred. The maximum number of items displayed by the block are configurable via the FeedDisplayMax property of the block (default = 20).

#### Block Configuration
To configure an Episerver Social feature block in a page do the following:

* Login to the SocialAlloy CMS edit panel
* Create a new block and select the "**&lt;feature&gt;** Block"
* Tweak any configuration in the block properties
* Publish the block
* Go to the page where the block functionality is desired
* Add/drag the block anywhere in the page
* Publish the modified page
* Viewing the page in the frontend should allow the use of that block's social feature

For implementation details of each of the Episerver Social blocks see the [source code](https://github.com/episerver/SocialAlloy/tree/master/src/EPiServer.SocialAlloy.Web/Social).

### Pages
#### Reseller Community Page

The **CommunityPage** represents a digital community which can be created for resellers of the Alloy platform. The page allows members of the reseller team to join the community. Site visitors can also comment on and rate the community page. Site users may subscribe to their favorite reseller communities and monitor a feed of activities from their personal profile.

## Developer Reference
Developers looking to get started with Episerver Social will find the [repository implementations](https://github.com/episerver/SocialAlloy/tree/master/src/EPiServer.SocialAlloy.Web/Social/Repositories) as the primary point of interaction between the application and the Episerver Social framework.

## Disclaimer
This website was assembled to serve as a demonstration and has not been tested for production use.

## More Information
For detailed information on how to implement social content solutions with Episerver Social, [please visit Episerver World](http://world.episerver.com/documentation/developer-guides/social/).

## Forum
Have questions, feature requests, or implementations that you'd like to share? Join the community on the [Episerver developer forum](http://world.episerver.com/forum/developer-forum/episerver-social/).

## Issues
If you encounter a problem, please [open a new issue](https://github.com/episerver/SocialAlloy/issues/new).

## Contributions
Please see our [contributing guidelines](https://github.com/episerver/SocialAlloy/blob/master/CONTRIBUTING).
