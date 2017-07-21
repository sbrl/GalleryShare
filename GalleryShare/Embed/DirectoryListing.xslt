<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="html" encoding="utf-8" indent="yes" />

	<xsl:output method="html" doctype-system="about:legacy-compat" />
	
	<xsl:template match="/">
		<html>
			<head>
				<meta name="viewport" content="width=device-width, initial-scale=1" />
				<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
				<title><xsl:value-of select="/DirectoryListing/CurrentDirectory" /> &#x2022; GalleryShare</title>
			</head>
			<body>
				<h1><xsl:value-of select="/DirectoryListing/CurrentDirectory" /></h1>
				
				<main>
					<xsl:apply-templates select="//ListingEntry" />
				</main>
				
				<footer>
					Powered by <a href="https://github.com/sbrl/GalleryShare">GalleryShare</a> &#x2022;
					Built by Starbeamrainbowlabs &#x2022;
					<img src="/!images/Badge-License.svg" />
				</footer>
				
				<link rel="stylesheet" href="/!Theme.css" />
			</body>
		</html>
	</xsl:template>
	
	<xsl:template match="ListingEntry[@Type='File']">
		<a href="{Name}" class="preview-backdrop">
			<figure class="preview file" style="background-image: url('{Name}?type=thumbnail');">
				<figcaption><xsl:value-of select="DisplayName" /></figcaption>
			</figure>
		</a>
	</xsl:template>
	
	<xsl:template match="ListingEntry[@Type='Directory']">
		<a href="{Name}" class="preview-backdrop">
			<figure class="preview directory" style="background-image: url('{Name}?type=thumbnail');">
				<figcaption><xsl:value-of select="DisplayName" /></figcaption>
			</figure>
		</a>
	</xsl:template>
</xsl:stylesheet>
