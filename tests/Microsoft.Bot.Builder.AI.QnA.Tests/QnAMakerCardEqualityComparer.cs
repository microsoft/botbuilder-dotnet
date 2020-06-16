// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.QnA.Tests
{
    public class QnAMakerCardEqualityComparer : IEqualityComparer<IActivity>
    {
        public bool Equals(IActivity x, IActivity y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            var activity1 = x.AsMessageActivity();
            var activity2 = y.AsMessageActivity();
 
            int activity1AttachmentCount = 0;
            int activity2AttachmentCount = 0;

            if (activity1?.Attachments != null)
            {
                activity1AttachmentCount = activity1.Attachments.Count;
            }

            if (activity2?.Attachments != null)
            {
                activity2AttachmentCount = activity2.Attachments.Count;
            }

            return (activity1AttachmentCount == activity2AttachmentCount) &&
                   string.Equals(activity1?.Text, activity2?.Text);
        }

        public int GetHashCode(IActivity obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
