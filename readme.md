# Sitecore.Support.142817
If you use 'IoC factory' in the pipeline with the following entry:

type="1" factory="ContainerFactory" ref="MyProject.MyPageResolver, MyProject"

You get an error: 
Exception Details: Sitecore.Exceptions.InvalidStructureException: No group node for sub node: httpRequestBegin

This patch allows using IoC factory in the pipeline.

## License  
This patch is licensed under the [Sitecore Corporation A/S License for GitHub](https://github.com/sitecoresupport/Sitecore.Support.142817/blob/master/LICENSE).  

## Download  
Downloads are available via [GitHub Releases](https://github.com/sitecoresupport/Sitecore.Support.142817/releases).  
