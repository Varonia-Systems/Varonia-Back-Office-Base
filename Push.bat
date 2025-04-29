@echo off
echo ===============================
echo  Pushing your changes to GitHub
echo ===============================
echo.

:: Demander un message de commit
set /p commitMessage="Entrez votre message de commit : "

:: Ajouter tous les fichiers modifiés
git add .

:: Commit avec le message donné
git commit -m "%commitMessage%"

:: Pousser sur la branche actuelle
git push

echo.
echo ✅ Push terminé !
pause
