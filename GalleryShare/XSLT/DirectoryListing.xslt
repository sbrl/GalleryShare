<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:template match="/">
		<html>
			<head>
				<title><xsl:value-of select="/DirectoryListing/CurrentDirectory" /> - GalleryShare</title>
			</head>
			<body>
				<h1><xsl:value-of select="/DirectoryListing/CurrentDirectory" /> - GalleryShare</h1>
				<p>This is a test</p>
				
				<!-- ---------- -->
				
				<style>
					html, body { font-size: 100%; }
					body
					{
						font-family: sans-serif;
					}
				</style>
			</body>
		</html>
	</xsl:template>
</xsl:stylesheet>
