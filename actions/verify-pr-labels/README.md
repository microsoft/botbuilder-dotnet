# Verify Pull Request Labels

This is a GitHub Action to verify that at least one label is assigned to a Pull Request.

```yml
- name: Verify Pull Request Labels
  uses: emmenko/action-verify-pr-labels@master
  with:
    github-token: '${{ secrets.GITHUB_TOKEN }}'
```
