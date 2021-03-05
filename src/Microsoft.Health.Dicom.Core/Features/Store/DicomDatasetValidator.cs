﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dicom;
using EnsureThat;
using Microsoft.Extensions.Options;
using Microsoft.Health.Dicom.Core.Configs;
using Microsoft.Health.Dicom.Core.Extensions;
using Microsoft.Health.Dicom.Core.Features.CustomTag;
using Microsoft.Health.Dicom.Core.Features.Validation;

namespace Microsoft.Health.Dicom.Core.Features.Store
{
    /// <summary>
    /// Provides functionality to validate a <see cref="DicomDataset"/> to make sure it meets the minimum requirement.
    /// </summary>
    public class DicomDatasetValidator : IDicomDatasetValidator
    {
        private readonly bool _enableFullDicomItemValidation;
        private readonly IDicomElementMinimumValidator _minimumValidator;
        private readonly IIndexTagService _indextagService;

        public DicomDatasetValidator(IOptions<FeatureConfiguration> featureConfiguration, IDicomElementMinimumValidator minimumValidator, IIndexTagService indexableDicomTagService)
        {
            EnsureArg.IsNotNull(featureConfiguration?.Value, nameof(featureConfiguration));
            EnsureArg.IsNotNull(minimumValidator, nameof(minimumValidator));
            EnsureArg.IsNotNull(indexableDicomTagService, nameof(indexableDicomTagService));

            _enableFullDicomItemValidation = featureConfiguration.Value.EnableFullDicomItemValidation;
            _minimumValidator = minimumValidator;
            _indextagService = indexableDicomTagService;
        }

        public async Task ValidateAsync(DicomDataset dicomDataset, string requiredStudyInstanceUid, CancellationToken cancellationToken)
        {
            EnsureArg.IsNotNull(dicomDataset, nameof(dicomDataset));

            // Ensure required tags are present.
            EnsureRequiredTagIsPresent(DicomTag.PatientID);
            EnsureRequiredTagIsPresent(DicomTag.SOPClassUID);

            // The format of the identifiers will be validated by fo-dicom.
            string studyInstanceUid = EnsureRequiredTagIsPresent(DicomTag.StudyInstanceUID);
            string seriesInstanceUid = EnsureRequiredTagIsPresent(DicomTag.SeriesInstanceUID);
            string sopInstanceUid = EnsureRequiredTagIsPresent(DicomTag.SOPInstanceUID);

            // Ensure the StudyInstanceUid != SeriesInstanceUid != sopInstanceUid
            if (studyInstanceUid == seriesInstanceUid ||
                studyInstanceUid == sopInstanceUid ||
                seriesInstanceUid == sopInstanceUid)
            {
                throw new DatasetValidationException(
                    FailureReasonCodes.ValidationFailure,
                    DicomCoreResource.DuplicatedUidsNotAllowed);
            }

            // If the requestedStudyInstanceUid is specified, then the StudyInstanceUid must match.
            if (requiredStudyInstanceUid != null &&
                !studyInstanceUid.Equals(requiredStudyInstanceUid, StringComparison.OrdinalIgnoreCase))
            {
                throw new DatasetValidationException(
                    FailureReasonCodes.MismatchStudyInstanceUid,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        DicomCoreResource.MismatchStudyInstanceUid,
                        studyInstanceUid,
                        requiredStudyInstanceUid));
            }

            string EnsureRequiredTagIsPresent(DicomTag dicomTag)
            {
                if (dicomDataset.TryGetSingleValue(dicomTag, out string value))
                {
                    return value;
                }

                throw new DatasetValidationException(
                    FailureReasonCodes.ValidationFailure,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        DicomCoreResource.MissingRequiredTag,
                        dicomTag.ToString()));
            }

            // validate input data elements
            if (_enableFullDicomItemValidation)
            {
                ValidateAllItems(dicomDataset);
            }
            else
            {
                await ValidateIndexedItems(dicomDataset, cancellationToken);
            }
        }

        private async Task ValidateIndexedItems(DicomDataset dicomDataset, CancellationToken cancellationToken)
        {
            IReadOnlyCollection<IndexTag> indexTags = await _indextagService.GetIndexTagsAsync(cancellationToken);

            HashSet<DicomTag> standardTags = indexTags.Select(indexTag => indexTag.Tag)
                .Where(tag => !tag.IsPrivate)
                .ToHashSet();

            IDictionary<string, DicomVR> privateTags = indexTags.Where(indexTag => indexTag.Tag.IsPrivate)
                .ToDictionary(indexTag => indexTag.Tag.GetPath(), indexTag => indexTag.VR);

            ValidateStandardTags(dicomDataset, standardTags);
            ValidatePrivateTags(dicomDataset, privateTags);
        }

        private void ValidateStandardTags(DicomDataset dicomDataset, HashSet<DicomTag> standardTags)
        {
            foreach (DicomTag indexableTag in standardTags)
            {
                DicomElement dicomElement = dicomDataset.GetDicomItem<DicomElement>(indexableTag);

                if (dicomElement != null)
                {
                    _minimumValidator.Validate(dicomElement);
                }
            }
        }

        private void ValidatePrivateTags(DicomDataset dicomDataset, IDictionary<string, DicomVR> privateTags)
        {
            // dicomDataset.GetDicomItem<DicomElement>() cannot get value for private tag, we need to loop and compare with path.
            foreach (DicomItem item in dicomDataset)
            {
                if (item.Tag.IsPrivate)
                {
                    // DicomTag from DicomDataset contains PrivateCreator, while the one from database doesn't have, need to remove private creator before comparision
                    string tagPath = item.Tag.GetPath();
                    if (privateTags.ContainsKey(tagPath) && privateTags[tagPath].Equals(item.ValueRepresentation))
                    {
                        DicomElement element = item as DicomElement;
                        if (element != null)
                        {
                            _minimumValidator.Validate(element);
                        }
                    }
                }
            }
        }

        private static void ValidateAllItems(DicomDataset dicomDataset)
        {
            dicomDataset.Each(item =>
            {
                item.ValidateDicomItem();
            });
        }
    }
}
