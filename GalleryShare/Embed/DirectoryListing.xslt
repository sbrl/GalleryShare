<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="html" encoding="utf-8" indent="yes" />
	<xsl:template match="/">
		<xsl:text disable-output-escaping="yes">&lt;!DOCTYPE html&gt;</xsl:text>
		<html>
			<head>
				<title><xsl:value-of select="/DirectoryListing/CurrentDirectory" /> - GalleryShare</title>
			</head>
			<body>
				<h1><xsl:value-of select="/DirectoryListing/CurrentDirectory" /> - GalleryShare</h1>
				
				<main>
					<xsl:apply-templates select="//ListingEntry" />
				</main>
				
				<link rel="stylesheet" href="/!Theme.css" />
			</body>
		</html>
	</xsl:template>
	
	<xsl:template match="ListingEntry[@Type='File']">
		<figure class='preview file'>
			<img src="{Name}?thumbnail" />
		</figure>
	</xsl:template>
	<xsl:template match="ListingEntry[@Type='Directory']">
		<figure class='preview directory'>
			(coming soon)
		</figure>
	</xsl:template>
</xsl:stylesheet>
