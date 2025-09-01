---
title: Migration
description: Guide to migrate from MediatR.CommandQuery to Arbiter
---

# Migration

The following is a guide to migrate from MediatR.CommandQuery to Arbiter.

Search and replace the following in order.

| Search Text                                | Replace Text                           | Description                               |
| ------------------------------------------ | -------------------------------------- | ----------------------------------------- |
| `MediatR.CommandQuery`                     | `Arbiter.CommandQuery`                 | Namesspace changes                        |
| `Arbiter.CommandQuery.EntityFrameworkCore` | `Arbiter.CommandQuery.EntityFramework` | EntityFramework namespace change          |
| `using MediatR;`                           | `using Arbiter.Mediation;`             | IMediator implementation assembly changed |

Search and replace using regular expressions

| Search Text                                              | Replace Text                                       | Description                                |
| -------------------------------------------------------- | -------------------------------------------------- | ------------------------------------------ |
| `protected override async Task\<([\w\<\>]+)\> Process\(` | `protected override async ValueTask<$1?> Process(` | Change to ValueTask for RequestHandlerBase |
