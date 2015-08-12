# Document.AWS

# What is Document.AWS?
- [AWS network EC2 instance documentation generator](https://trov-dev.ghost.io/documenting-jello-how-we-automated-our-infrastructure-documentation "The Cache - Trov Engineering Blog")
- Fully configurable for all current AWS regions
- Fully customizable CSS, JavaScript, colors and images

So there are a few things you will need to configure before it is ready to use on your AWS environment(s):

## aws.environments ##
First one we will discuss here is the file that describes your AWS environments. By default it is called **aws.environments**, but you can rename it to whatever you like (Make sure you change the entry in **App.config** as well!)

Here is a sample **aws.environments** file:

    <AWSEnvironments>
      <List>
        <AWSEnvironment>
          <AccessKeyID>ABCD1234EFGH5678IJKL</AccessKeyID>
          <SecretAccessKey>a1b2c3d4e5g6h7i8j9k0l1m2n3o4p5q6r7s8t9u0</SecretAccessKey>
          <HTMLPage>development-us-west-1.html</HTMLPage>
          <Region>us-west-1</Region>
          <Name>Development</Name>
        </AWSEnvironment>
        ...
      </List>
    </AWSEnvironments>

The **AccessKeyID** and **SecretAccessKey** elements should be self-explanatory. Ensure that the IAM user they represent has both EC2 and VPC read access.

The **HTMLPage** element is the filename you wish to create.

The **Region** element is any one of the 9 valid AWS regions (you can try US GovCloud if you have access, but since I do not, you're on your own!)

The **Name** element is the title you want displayed for your HTML page.

You can have as many **AWSEnvironment** entries as you wish. If there are no EC2 instances in the listed region, no HTML page will be created.

## aws.icons ##
The file **aws.icons** contains the server icon images you wish to use, along with their correlating roles, which you need to tag your EC2 instances with:

Here is a sample **aws.icons** file:

    <ServerIcons>
      <List>
        <ServerIcon>
          <Role>UNKNOWN</Role>
          <Icon>./images/unknownserver.png</Icon>
        </ServerIcon>
        <ServerIcon>
          <Role>APACHE</Role>
          <Icon>./images/apacheserver.png</Icon>
        </ServerIcon>
        ...
      </List>
    </ServerIcons>

The **Role** element is one of the roles you define. They can be anything you like, the included file contains some generic ones you may find useful. If not, remove any you don't feel you need. You will need to add a **Role** tag to any EC2 instance, using the AWS console, that you want an image displayed for.

The **Icon** element is the relative location of the image file you want displayed for any servers that are tagged in AWS with this **Role**. If you add your own images they should be the same size as the ones contained here, namely 105 x 105px.

The **UNKNOWN** role should not be removed or the resulting HTML pages probably won't look right.

## aws.css ##
The file **aws.css** contains all the CSS style elements required by the generated HTML pages. Feel free to edit it how you want it, but if you remove any styles the resulting HTML pages probably won't look right. Again, you can rename it to whatever you like, just make sure you change the entry in **App.config** as well.

## aws.scripts ##
The aws.scripts file is more of a future placeholder file (for our environment anyway). As you might have surmised from the name, it's for any JavaScript you may need on your pages. It is blank right now, but feel free to add any JavaScript you want to use.

## index.html ##
Feel free to edit or completely replace this file, it's just a placeholder to show your the 9 AWS region locations with links to your generated HTML files.

## AWS Tags ##
**Role**

As explained above, it is a required tag you will need to add to each of your AWS instances in order to have the correct server icon image displayed for it. If it doesn't exist your server will just display whatever you defined for **UNKNOWN** in your **aws.icons** file.

**Description**

Not a required tag, but if it is defined, will display whatever text you enter as a description of the AWS instance in the generated HTML page.

**Owner**

Also not a required tag, but if it is defined, will display whatever name you enter as the owner of the AWS instance in the generated HTML page. We use this here in the event you need to talk to whomever is responsible for the 
 
## Documenting Your Instances ##
Once you have compiled the source and edited the files as shown above, simply run the file **Document.AWS.exe** and wait for it to complete. If on the off chance something goes wrong, there will likely be a clue in the log files that will be generated, aptly named **Document.AWS.log** or **Document.AWS.Connect.log**.

## Copyright and license ##

Code and documentation copyright 2015 Trov Inc. Code is released under the [MIT license](https://github.com/Trov/document.aws/blob/master/LICENSE "document.aws code license"). Documentation is released under the [Creative Commons license](https://github.com/Trov/document.aws/blob/master/LICENSE-DOCS "document.aws documentation license").