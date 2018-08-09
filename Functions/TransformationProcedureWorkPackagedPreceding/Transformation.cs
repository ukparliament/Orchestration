using Parliament.Model;
using Parliament.Rdf.Serialization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using VDS.RDF;

namespace Functions.TransformationProcedureWorkPackagedPreceding
{
    public class Transformation : BaseTransformationSqlServer<Settings, DataSet>
    {
        public override BaseResource[] TransformSource(DataSet dataset)
        {
            ProposedNegativeStatutoryInstrumentPaper proposedNegativeStatutoryInstrument = new ProposedNegativeStatutoryInstrumentPaper();
            DataRow row = dataset.Tables[0].Rows[0];
            Uri idUri = GiveMeUri(GetText(row["ProposedNegativeStatutoryInstrument"]));
            if (idUri == null)
                return null;
            else
                proposedNegativeStatutoryInstrument.Id = idUri;
            if (Convert.ToBoolean(row["IsDeleted"]))
                return new BaseResource[] { proposedNegativeStatutoryInstrument };

            Uri statutoryInstrumentUri = GiveMeUri(GetText(row["StatutoryInstrument"]));
            if (statutoryInstrumentUri == null)
                return null;
            else
                proposedNegativeStatutoryInstrument.ProposedNegativeStatutoryInstrumentPaperPrecedesStatutoryInstrumentPaper = new List<StatutoryInstrumentPaper>()
                {
                    new StatutoryInstrumentPaper()
                    {
                        Id = statutoryInstrumentUri
                    }
                };

            return new BaseResource[] { proposedNegativeStatutoryInstrument };
        }

        public override Uri GetSubjectFromSource(BaseResource[] deserializedSource)
        {
            return deserializedSource.OfType<ProposedNegativeStatutoryInstrumentPaper>()
                .SingleOrDefault()
                .Id;
        }

        public override BaseResource[] SynchronizeIds(BaseResource[] source, Uri subjectUri, BaseResource[] target)
        {
            ProposedNegativeStatutoryInstrumentPaper proposedNegativeStatutoryInstrumentPaper = source.OfType<ProposedNegativeStatutoryInstrumentPaper>()
                .SingleOrDefault();
            ProposedNegativeStatutoryInstrumentPaper targetStatutoryInstrumentPaper = target.OfType<ProposedNegativeStatutoryInstrumentPaper>()
                .SingleOrDefault();
            if ((targetStatutoryInstrumentPaper != null) && (targetStatutoryInstrumentPaper.ProposedNegativeStatutoryInstrumentPaperPrecedesStatutoryInstrumentPaper.Any()))
            {
                if ((proposedNegativeStatutoryInstrumentPaper.ProposedNegativeStatutoryInstrumentPaperPrecedesStatutoryInstrumentPaper != null) && (proposedNegativeStatutoryInstrumentPaper.ProposedNegativeStatutoryInstrumentPaperPrecedesStatutoryInstrumentPaper.Any()))
                {
                    List<StatutoryInstrumentPaper> statutoryInstrumentPapers = new List<StatutoryInstrumentPaper>();
                    statutoryInstrumentPapers.AddRange(proposedNegativeStatutoryInstrumentPaper.ProposedNegativeStatutoryInstrumentPaperPrecedesStatutoryInstrumentPaper);
                    statutoryInstrumentPapers.AddRange(targetStatutoryInstrumentPaper.ProposedNegativeStatutoryInstrumentPaperPrecedesStatutoryInstrumentPaper);
                    proposedNegativeStatutoryInstrumentPaper.ProposedNegativeStatutoryInstrumentPaperPrecedesStatutoryInstrumentPaper = statutoryInstrumentPapers;
                }
                else
                    proposedNegativeStatutoryInstrumentPaper.ProposedNegativeStatutoryInstrumentPaperPrecedesStatutoryInstrumentPaper = targetStatutoryInstrumentPaper.ProposedNegativeStatutoryInstrumentPaperPrecedesStatutoryInstrumentPaper;
            }
            return new BaseResource[] { proposedNegativeStatutoryInstrumentPaper };
        }

        public override IGraph AlterNewGraph(IGraph newGraph, Uri id, DataSet response)
        {
            ProposedNegativeStatutoryInstrumentPaper proposedNegativeStatutoryInstrument = new ProposedNegativeStatutoryInstrumentPaper();
            DataRow row = response.Tables[0].Rows[0];

            if (Convert.ToBoolean(row["IsDeleted"]))
            {
                Uri statutoryInstrumentUri = GiveMeUri(GetText(row["StatutoryInstrument"]));
                Triple deletedTriple = newGraph.GetTriplesWithObject(statutoryInstrumentUri)
                    .SingleOrDefault();
                if (deletedTriple != null)
                    newGraph.Retract(deletedTriple);
            }

            return newGraph;
        }

    }
}
