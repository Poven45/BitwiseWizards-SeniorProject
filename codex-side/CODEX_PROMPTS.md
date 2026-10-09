# Codex prompts for the SEO demo

Codex only writes code on a local branch. You push the branch and open the pull request yourself. That keeps your GitHub login away from Codex, and Codex's sandbox usually has no network access anyway.

Run these inside your Trust Trade repo, after `fetch-report.sh` has copied the report to `.hermes-report/`.

## 1. Fix one thing at a time

```
Read .hermes-report/<report file>.
Do task 1 only (the page title).
Create the branch hermes/seo-title from hermes-dev.
Change as little as possible, run the project's tests, and tell me what you changed.
Do not push.
```

One task per branch means one small pull request per decision. You can approve the title fix and reject the alt-text fix without untangling them.

## 2. Review before you push

```
git diff hermes-dev...hermes/seo-title
```

Read the diff. The change should touch only the file that holds the page title. If it touches anything else (config, scripts, packages, `.github/`), reject it.

## 3. Push and open the pull request yourself

```
git push -u origin hermes/seo-title
```

Then open a pull request from `hermes/seo-title` into `hermes-dev` on GitHub. Merging it is the approval. Closing it without merging is the rejection.
