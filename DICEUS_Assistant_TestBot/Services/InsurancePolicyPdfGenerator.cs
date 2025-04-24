﻿using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICEUS_Assistant_TestBot.Services;

public class InsurancePolicyPdfGenerator
{
	// Creates a PDF document from the provided text (insurance policy)
	public static byte[] CreateFromText(string policyText)
	{
		// Set the license type for QuestPDF
		QuestPDF.Settings.License = LicenseType.Community;

		// Create the PDF document
		var document = Document.Create(container =>
		{
			container.Page(page =>
			{
				page.Size(PageSizes.A4);

				page.Margin(50);

				page.DefaultTextStyle(x => x.FontSize(14).FontFamily("Arial"));

				page.Header()
					.Text("Car Insurance Policy")
					.FontSize(20)
					.Bold()
					.AlignCenter();

				page.Content()
					.PaddingVertical(20)
					.Column(column =>
					{
						// Insert the policy text as a paragraph
						column.Item().Text(policyText);
					});

				// Create the footer with a generation date
				page.Footer()
					.AlignCenter()
					.Text($"Generated on {DateTime.Now:yyyy-MM-dd}");
			});
		});

		return document.GeneratePdf();
	}
}
