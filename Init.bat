@echo off
setlocal

echo ================================
echo  Initialisation du projet Git
echo ================================
echo.

:: Demande le nom du dépôt Git
set /p repoName="Nom du dépôt Git (ex: Varonia-Back-Office-Base) : "

:: Étape 1 : Initialiser Git
git init

:: Étape 2 : Créer un .gitignore adapté à Unity
echo # .gitignore pour projet Unity > .gitignore
echo [Ll]ibrary/ >> .gitignore
echo [Tt]emp/ >> .gitignore
echo [Oo]bj/ >> .gitignore
echo [Bb]uild/ >> .gitignore
echo [Bb]uilds/ >> .gitignore
echo [Ll]ogs/ >> .gitignore
echo [Uu]serSettings/ >> .gitignore
echo .vs/ >> .gitignore
echo *.csproj >> .gitignore
echo *.unityproj >> .gitignore
echo *.sln >> .gitignore
echo *.suo >> .gitignore
echo *.tmp >> .gitignore
echo *.user >> .gitignore
echo *.userprefs >> .gitignore
echo *.pidb >> .gitignore
echo *.booproj >> .gitignore
echo *.svd >> .gitignore
echo *.pdb >> .gitignore
echo *.mdb >> .gitignore
echo *.opendb >> .gitignore
echo *.VC.db >> .gitignore
echo .DS_Store >> .gitignore
echo .idea/ >> .gitignore
echo *.apk >> .gitignore
echo *.aab >> .gitignore
echo push.bat >> .gitignore
echo Init.bat >> .gitignore
echo init-unity-git.bat >> .gitignore

echo ✅ .gitignore généré.

:: Étape 3 : Ajouter les fichiers
git add .

:: Étape 4 : Commit initial
git commit -m "First"

:: Étape 5 : Ajouter le remote GitHub
git branch -M main
git remote add origin https://github.com/Varonia-Systems/%repoName%.git

:: Étape 6 : Push initial
git push -u origin main

echo.
echo ✅ Dépôt en ligne prêt : https://github.com/Varonia-Systems/%repoName%
echo.
pause
