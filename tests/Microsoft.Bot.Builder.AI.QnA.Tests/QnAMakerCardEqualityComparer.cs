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

            if (activity1 == null || activity2 == null)
            {
                return false;
            }

            //Return false if only one of the attachments is null.
            if ((activity1.Attachments == null && activity2.Attachments != null) ||
                (activity1.Attachments != null && activity2.Attachments == null))
            {
                return false;
            }

            // Check for attachments only if neither of the attachments are null.
            if (activity1.Attachments != null && activity2.Attachments != null)
            {
                if (activity1.Attachments.Count != activity2.Attachments.Count)
                {
                    return false;
                }
            }

            if (!string.Equals(activity1.Text, activity2.Text))
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(IActivity obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
