The steps to get this WinForms project working are:

1. Open Program.cs and replace the code to create IoC container:
	new Bootstrapper<MainViewModel>(new IIocContainer())
2. Remove any old forms