using Google.Api.Ads.AdWords.Lib;
using Google.Api.Ads.AdWords.Util.Selectors;
using Google.Api.Ads.AdWords.v201809;
using QuarterMaster.Debugging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace QuarterMaster.GoogleAdWords
{
    public static class AdWords
    {
        public static StringBuilder GetTree(AdWordsUser user)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }
            // Get the ManagedCustomerService.
            ManagedCustomerService managedCustomerService = (ManagedCustomerService)user.GetService(
                AdWordsService.v201809.ManagedCustomerService);

            // Create selector.
            Selector selector = new Selector
            {
                fields = new String[] {
                ManagedCustomer.Fields.CustomerId, ManagedCustomer.Fields.Name
            },
                paging = Paging.Default
            };

            // Map from customerId to customer node.
            Dictionary<long, ManagedCustomerTreeNode> customerIdToCustomerNode =
                new Dictionary<long, ManagedCustomerTreeNode>();

            // Temporary cache to save links.
            List<ManagedCustomerLink> allLinks = new List<ManagedCustomerLink>();

            ManagedCustomerPage page;
            try
            {
                do
                {
                    page = managedCustomerService.get(selector);

                    if (page.entries != null)
                    {
                        // Create account tree nodes for each customer.
                        foreach (ManagedCustomer customer in page.entries)
                        {
                            ManagedCustomerTreeNode node = new ManagedCustomerTreeNode
                            {
                                Account = customer
                            };
                            customerIdToCustomerNode.Add(customer.customerId, node);
                        }

                        if (page.links != null)
                        {
                            allLinks.AddRange(page.links);
                        }
                    }
                    selector.paging.IncreaseOffset();
                } while (selector.paging.startIndex < page.totalNumEntries);

                // For each link, connect nodes in tree.
                foreach (ManagedCustomerLink link in allLinks)
                {
                    ManagedCustomerTreeNode managerNode =
                        customerIdToCustomerNode[link.managerCustomerId];
                    ManagedCustomerTreeNode childNode = customerIdToCustomerNode[link.clientCustomerId];
                    childNode.ParentNode = managerNode;
                    if (managerNode != null)
                    {
                        managerNode.ChildAccounts.Add(childNode);
                    }
                }

                // Find the root account node in the tree.
                ManagedCustomerTreeNode rootNode = null;
                foreach (ManagedCustomerTreeNode node in customerIdToCustomerNode.Values)
                {
                    if (node.ParentNode == null)
                    {
                        rootNode = node;
                        break;
                    }
                }

                // Display account tree.
                //Console.WriteLine("CustomerId, Name");
                //Console.WriteLine(rootNode.ToTreeString(0, new StringBuilder()));
                //return rootNode;
                return rootNode.ToTreeString3(new StringBuilder());
            }
            catch (Exception e)
            {
                throw new System.ApplicationException("Failed to create ad groups.", e);
            }
        }

        public static ManagedCustomer GetManagedCustomer(AdWordsUser user, string clientCustomerId)
        {
            // Get the ManagedCustomerService.
            var managedCustomerService = (ManagedCustomerService)user.GetService(
                AdWordsService.v201809.ManagedCustomerService);

            // Create selector.
            Selector selector = new Selector
            {
                //fields = (string[])ManagedCustomer.Fields.All.ToList<Field>().Select(i => i.FieldName).ToArray<string>(),
                fields = new string[] { ManagedCustomer.Fields.Name, ManagedCustomer.Fields.CustomerId },
                paging = Paging.Default,
                predicates = new Predicate[] { Predicate.Equals(ManagedCustomer.Fields.CustomerId, clientCustomerId) }
            };

            var managedCustomer = managedCustomerService.get(selector);

            return managedCustomer.entries?[0];
        }

        public static List<AdGroupAd> GetAdGroupAds(AdWordsUser user, long adGroupId)
        {
            var list = new List<AdGroupAd>();
            // Get the CampaignService.
            var adGroupsAdService =
                (AdGroupAdService)user.GetService(AdWordsService.v201809.AdGroupAdService);

            // Create the selector.
            var selector = new Selector()
            {
                //fields = (string[])AdGroupAd.Fields.All.ToList<Field>().Select(i => i.FieldName).ToArray<string>(),
                fields = new string[]
                {
                    "Id",
                    "Url",
                    "DisplayUrl",
                    "CreativeFinalUrls",
                    "CreativeFinalMobileUrls",
                    "CreativeFinalAppUrls",
                    "CreativeTrackingUrlTemplate",
                    "CreativeUrlCustomParameters",
                    "UrlData",
                    "AdType",
                    "DevicePreference"
                },
                predicates = new Predicate[] { Predicate.Equals(AdGroupAd.Fields.AdGroupId, adGroupId) },
                paging = Paging.Default
            };

            AdGroupAdPage page;

            try
            {
                do
                {
                    // Get the campaigns.
                    page = adGroupsAdService.get(selector);

                    // Display the results.
                    if (page?.entries != null)
                    {
                        int i = selector.paging.startIndex;
                        foreach (AdGroupAd adGroupAd in page.entries)
                        {
                            list.Add(adGroupAd);
                            i++;
                        }
                    }
                    selector.paging.IncreaseOffset();
                } while (selector.paging.startIndex < page.totalNumEntries);
            }
            catch (Exception e)
            {
                throw new System.ApplicationException("Failed to retrieve adgroupads", e);
            }
            return list;
        }

        public static List<AdGroupAd> GetAdGroupAdListed(AdWordsUser user, long adGroupId, long adId)
        {
            var list = new List<AdGroupAd>();
            // Get the CampaignService.
            var adGroupsAdService =
                (AdGroupAdService)user.GetService(AdWordsService.v201809.AdGroupAdService);

            // Create the selector.
            var selector = new Selector()
            {
                fields = (string[])Ad.SelectableFields.All.ToList<Field>().Select(i => i.FieldName).ToArray<string>(),
                predicates = new Predicate[] {
                    Predicate.Equals(AdGroupAd.Fields.AdGroupId, adGroupId),
                    Predicate.Equals(Ad.Fields.Id,adId)},
                paging = Paging.Default
            };

            var page = new AdGroupAdPage();

            try
            {
                do
                {
                    // Get the campaigns.
                    page = adGroupsAdService.get(selector);

                    // Display the results.
                    if (page?.entries != null)
                    {
                        int i = selector.paging.startIndex;
                        foreach (AdGroupAd adGroupAd in page.entries)
                        {
                            list.Add(adGroupAd);
                            i++;
                        }
                    }
                    selector.paging.IncreaseOffset();
                } while (selector.paging.startIndex < page.totalNumEntries);
            }
            catch (Exception e)
            {
                throw new System.ApplicationException("Failed to retrieve adgroupads", e);
            }
            return list;
        }

        public static AdGroupAd GetAdGroupAd(AdWordsUser user, long adGroupId, long adId)
        {
            AdGroupAd adGroupAd = null;

            // Get the CampaignService.
            var adGroupsAdService =
                (AdGroupAdService)user.GetService(AdWordsService.v201809.AdGroupAdService);

            // Create the selector.
            var selector = new Selector()
            {
                fields = (string[])Ad.SelectableFields.All.ToList<Field>().Select(i => i.FieldName).ToArray<string>(),
                predicates = new Predicate[] {
                    Predicate.Equals(AdGroupAd.Fields.AdGroupId, adGroupId),
                    Predicate.Equals(Ad.Fields.Id,adId)},
                paging = Paging.Default
            };

            var page = new AdGroupAdPage();

            try
            {
                do
                {
                    // Get the campaigns.
                    page = adGroupsAdService.get(selector);

                    // Display the results.
                    if (page?.entries != null)
                    {
                        int i = selector.paging.startIndex;
                        foreach (AdGroupAd adJroupAd in page.entries)
                        {
                            adGroupAd = adJroupAd;
                            i++;
                        }
                    }
                    selector.paging.IncreaseOffset();
                } while (selector.paging.startIndex < page.totalNumEntries);
            }
            catch (Exception e)
            {
                throw new System.ApplicationException("Failed to retrieve adgroupads", e);
            }
            return adGroupAd;
        }

        public static Campaign GetCampaign(AdWordsUser user, long campaignId)
        {
            Campaign campaign = null;
            // Get the CampaignService.
            var campaignService =
                (CampaignService)user.GetService(AdWordsService.v201809.CampaignService);

            // Create the selector.
            var selector = new Selector()
            {
                fields = (string[])Campaign.SelectableFields.All.ToList<Field>().Select(i => i.FieldName).ToArray<string>(),
                paging = Paging.Default,
                predicates = new Predicate[] { Predicate.Equals(Campaign.Fields.Id, campaignId) }
            };

            var page = new CampaignPage();

            try
            {
                do
                {
                    // Get the campaigns.
                    page = campaignService.get(selector);

                    // Display the results.
                    if (page?.entries != null)
                    {
                        int i = selector.paging.startIndex;
                        foreach (Campaign kampaign in page.entries)
                        {
                            campaign = kampaign;
                            i++;
                        }
                    }
                    selector.paging.IncreaseOffset();
                } while (selector.paging.startIndex < page.totalNumEntries);
            }
            catch (Exception e)
            {
                throw new System.ApplicationException("Failed to retrieve campaigns", e);
            }
            return campaign;
        }

        public static List<Campaign> GetCampaigns(AdWordsUser user)
        {
            var list = new List<Campaign>();
            // Get the CampaignService.
            var campaignService =
                (CampaignService)user.GetService(AdWordsService.v201809.CampaignService);

            // Create the selector.
            var selector = new Selector()
            {
                fields = (string[])Campaign.SelectableFields.All.ToList<Field>().Select(i => i.FieldName).ToArray<string>(),
                paging = Paging.Default
            };

            var page = new CampaignPage();

            try
            {
                do
                {
                    // Get the campaigns.
                    page = campaignService.get(selector);

                    // Display the results.
                    if (page?.entries != null)
                    {
                        int i = selector.paging.startIndex;
                        foreach (Campaign campaign in page.entries)
                        {
                            list.Add(campaign);
                            i++;
                        }
                    }
                    selector.paging.IncreaseOffset();
                } while (selector.paging.startIndex < page.totalNumEntries);
            }
            catch (Exception e)
            {
                throw new System.ApplicationException("Failed to retrieve campaigns", e);
            }
            return list;
        }

        public static AdGroup GetAdGroup(AdWordsUser user, long campaignId, long adgroupId)
        {
            AdGroup adGroup = null;

            // Get the AdGroupService.
            var adGroupService =
                (AdGroupService)user.GetService(AdWordsService.v201809.AdGroupService);

            // Create the selector.
            var selector = new Selector()
            {
                fields = (string[])AdGroup.SelectableFields.All.ToList<Field>().Select(i => i.FieldName).ToArray<string>(),
                predicates = new Predicate[] { Predicate.Equals(AdGroup.Fields.CampaignId, campaignId),
                Predicate.Equals(AdGroup.Fields.Id,adgroupId)},
                paging = Paging.Default,
                ordering = new OrderBy[] { OrderBy.Asc(AdGroup.Fields.Name) }
            };

            var page = new AdGroupPage();

            try
            {
                do
                {
                    // Get the ad groups.
                    page = adGroupService.get(selector);

                    // Display the results.
                    if (page?.entries != null)
                    {
                        int i = selector.paging.startIndex;
                        foreach (AdGroup adGroup1 in page.entries)
                        {
                            adGroup = adGroup1;
                            i++;
                        }
                    }
                    // Note: You can also use selector.paging.IncrementOffsetBy(customPageSize)
                    selector.paging.IncreaseOffset();
                } while (selector.paging.startIndex < page.totalNumEntries);
            }
            catch (Exception e)
            {
                throw new System.ApplicationException("Failed to retrieve ad groups.", e);
            }
            return adGroup;
        }

        public static List<AdGroup> GetAdGroups(AdWordsUser user, long campaignId)
        {
            var list = new List<AdGroup>();
            // Get the AdGroupService.
            var adGroupService =
                (AdGroupService)user.GetService(AdWordsService.v201809.AdGroupService);

            // Create the selector.
            var selector = new Selector()
            {
                fields = (string[])AdGroup.SelectableFields.All.ToList<Field>().Select(i => i.FieldName).ToArray<string>(),
                predicates = new Predicate[] { Predicate.Equals(AdGroup.Fields.CampaignId, campaignId) },
                paging = Paging.Default,
                ordering = new OrderBy[] { OrderBy.Asc(AdGroup.Fields.Name) }
            };

            var page = new AdGroupPage();

            try
            {
                do
                {
                    // Get the ad groups.
                    page = adGroupService.get(selector);

                    // Display the results.
                    if (page?.entries != null)
                    {
                        int i = selector.paging.startIndex;
                        foreach (AdGroup adGroup in page.entries)
                        {
                            list.Add(adGroup);
                            i++;
                        }
                    }
                    // Note: You can also use selector.paging.IncrementOffsetBy(customPageSize)
                    selector.paging.IncreaseOffset();
                } while (selector.paging.startIndex < page.totalNumEntries);
            }
            catch (Exception e)
            {
                throw new System.ApplicationException("Failed to retrieve ad groups.", e);
            }
            return list;
        }

        public static BillingAccount[] GetBillingAccounts(AdWordsUser user)
        {
            var bos = (BudgetOrderServiceInterface)user.GetService(AdWordsService.v201809.BudgetOrderService);
            var bar = new Google.Api.Ads.AdWords.v201809.Wrappers.BudgetOrderService.getBillingAccountsRequest();
            var ba = bos.getBillingAccounts(bar);
            return ba.rval;
        }

        public static string GetChangeHistory(AdWordsUser user, string dateStart, string dateEnd)
        {
            if (DebugPoints.DebugPointRequested(MethodBase.GetCurrentMethod().Name.ToUpper()))
            {
                Debugger.Launch();
            }
            var campaignService =
                (CampaignService)user.GetService(AdWordsService.v201809.CampaignService);
            var customerSyncService =
                (CustomerSyncService)user.GetService(AdWordsService.v201809.CustomerSyncService);

            // Get a list of all campaign IDs.
            var campaignIds = new List<long>();
            var selector = new Selector()
            {
                fields = new string[] { Campaign.Fields.Id }
            };
            var campaigns = campaignService.get(selector);
            if (campaigns != null)
            {
                foreach (Campaign campaign in campaigns.entries)
                {
                    campaignIds.Add(campaign.id);
                }
            }

            var dateTimeRange = new DateTimeRange
            {
                max = dateEnd,
                min = dateStart
            };

            // Create selector.
            var customerSyncSelector = new CustomerSyncSelector
            {
                dateTimeRange = dateTimeRange,
                campaignIds = campaignIds.ToArray<long>()
            };

            // Get all account changes for campaign.
            var accountChanges = customerSyncService.get(customerSyncSelector);

            if (accountChanges == null || accountChanges.changedCampaigns == null)
            {
                return "No changes in period.";
            }

            var sb = new StringBuilder();
            sb.Append("Most recent change: ").AppendLine(accountChanges.lastChangeTimestamp);

            foreach (CampaignChangeData campaignChangeData in accountChanges.changedCampaigns)
            {
                sb.Append("Campaign with ID ").Append(campaignChangeData.campaignId).AppendLine(" was changed:")
                    .Append("\tCampaign changed status: '").Append(campaignChangeData.campaignChangeStatus).AppendLine("'");
                if (!ChangeStatus.NEW.Equals(campaignChangeData.campaignChangeStatus))
                {
                    sb.Append("\tAdded campaign criteria: ").Append(campaignChangeData.addedCampaignCriteria).AppendLine()
                        .Append("\tChanged campaign criteria: ").Append(campaignChangeData.removedCampaignCriteria).AppendLine();
                    if (campaignChangeData.changedAdGroups != null)
                    {
                        foreach (AdGroupChangeData adGroupChangeData in campaignChangeData.changedAdGroups)
                        {
                            sb.Append("\tAdGroup with ID ").Append(adGroupChangeData.adGroupId).AppendLine(" was changed:")
                                .Append("\t\tAdGroup changed status: ").Append(adGroupChangeData.adGroupChangeStatus).AppendLine();
                            if (!ChangeStatus.NEW.Equals(adGroupChangeData.adGroupChangeStatus))
                            {
                                sb.Append("\t\tAds changed: ").Append(adGroupChangeData.changedAds).AppendLine()
                                    .Append("\t\tCriteria changed: ").Append(adGroupChangeData.changedCriteria).AppendLine()
                                    .Append("\t\tCriteria removed: ").Append(adGroupChangeData.removedCriteria).AppendLine();
                            }
                        }
                    }
                }
            }

            return sb.ToString();
        }
    }
}
