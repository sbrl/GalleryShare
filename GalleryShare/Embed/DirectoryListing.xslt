<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="html" encoding="utf-8" indent="yes" />
	<xsl:template match="/">
		<xsl:text disable-output-escaping="yes">&lt;!DOCTYPE html&gt;</xsl:text>
		<html>
			<head>
				<meta name="viewport" content="width=device-width, initial-scale=1" />
				<title><xsl:value-of select="/DirectoryListing/CurrentDirectory" /> <xsl:text disable-output-escaping="yes"><![CDATA[ &#x95; ]]></xsl:text> GalleryShare</title>
			</head>
			<body>
				<h1><xsl:value-of select="/DirectoryListing/CurrentDirectory" /></h1>
				
				<main>
					<xsl:apply-templates select="//ListingEntry" />
				</main>
				
				<footer>
					Built by Starbeamrainbowlabs
					<img src="/!images/Badge-License.svg" />
				</footer>
				
				<link rel="stylesheet" href="/!Theme.css" />
			</body>
		</html>
	</xsl:template>
	
	<xsl:template match="ListingEntry[@Type='File']">
		<span class="preview-backdrop">
			<figure class="preview file" style="background-image: url('{Name}?type=thumbnail');">
				<figcaption><xsl:value-of select="Name" /></figcaption>
			</figure>
		</span>
	</xsl:template>
	<xsl:template match="ListingEntry[@Type='Directory']">
		<span class="preview-backdrop">
			<figure class="preview directory">
				(coming soon)
				<figcaption><xsl:value-of select="Name" /></figcaption>
			</figure>
		</span>
	</xsl:template>
</xsl:stylesheet>
