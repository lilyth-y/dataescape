## Cursor Cloud specific instructions

- This is a Unity project for **Memory Fracture**. Standard setup/run instructions live in `README.md`; Unity/Git LFS notes live in `UNITY_GIT_SETUP.md`.
- The project targets Unity `2022.3.21f1`. In this Cloud environment, the editor is installed at `/opt/unity/2022.3.21f1/Unity`.
- The startup update script should only refresh LFS objects with `git lfs pull`. Running `git lfs install --local` conflicts with Cursor's managed Git pre-push hook in this repo.
- Unity batchmode import, tests, builds, and play-mode runs require a valid Unity license (`UNITY_LICENSE` ULF content or another Unity-supported activation path) before they can complete in Cloud.
