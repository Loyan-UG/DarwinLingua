@echo off
setlocal
"%npm_node_execpath%" "%~dp0..\node_modules\tailwindcss\lib\cli.js" -i "%~dp0..\Styles\tailwind.css" -o "%~dp0..\wwwroot\css\tailwind.generated.css" --minify
