#####What is Conveyor?

[![Join the chat at https://gitter.im/AliSheikhTaheri/Conveyor](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/AliSheikhTaheri/Conveyor?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

Conveyor allows editors and developers to transfer content (media and content) of a website from one environment to another.  

For a full demonstration please view the screencast. www.screenr.com/05tN

#####What are the use case scenario?

Developers, especially front-ends, want to work with live content so content can be transferred from live to local machine.
A new section is developed for an existing site which is not on the live site yet and client would like to preview / approve all the content before it goes live. Therefore content can be entered in UAT server and then can be transferred to live.

#####How does it work?

The package creates a zip file based on the chosen content which then can be imported to another environment.

Conveyor looks at a page and export all the associated pages / media with it. 

Imagine you would like to export a news article that has several categories selected by MNTP, an image inside RTE and a header image which is stored in media section under images/news images folder. 

When the above page is selected to be exported Conveyor automatically exports the header image, the image that is used inside RTE and all the categories that are associated with this page. It means when the news article page is imported to new environment the page is built with all the associated data. Magic isn't it!

#####What are the assumptions?

All environments must have the same document types, data types, and templates. Remember the package is only responsible for content and not anything else. 
Node dependency is only one level in content, however there is no limit for media files which means the package exports / imports all the levels and dependencies for media files.
How does it work behind the scene?

Basically Data types can be classified into two categories:  

Special data types: the data types that store a reference or id of a page or media.
ex: Multi Node Tree Picker (MNTP), media picker, content picker
Other data types: the data types that store text ex: text string, true/false
"Other Data Types" will work out of the box and no conversion is needed for these types.

However "Special Data Types" as its names implies are special and they are storing an Id of another page or media file. When the value of these data types are transferred to new environment (website/server) they need to be updated with the new Id therefore a conversion is needed for these types.

This package does the conversion for the following special data types:

MNTP
Content Picker
Media Picker
Drop Down List
Rich Text Editor 
The conversion for above data types can be swapped with custom data type convertor in ContentConveyor.config file. You can create your own class which inherits from BaseContentManagement and implements IDataTypeConverter which has two methods, export and import.

In the Export method you need to convert the page/media Id into Guid and in the Import method the Guid should be converted back to Id. 

If you have a custom data type which stores a reference to a page or a media file and you needed a conversion and it can be added to ContentConveyor.config file. You can also check out the source code for all the above data types to see how they are implemented.

#####Is Conveyor open-source?

Yes, it is. the source code can be found on my GitHub account. www.github.com/AliSheikhTaheri/Conveyor

#####*** Make sure you back up the database first before you import content ***

