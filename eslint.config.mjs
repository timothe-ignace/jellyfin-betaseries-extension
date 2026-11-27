import js from "@eslint/js";
import html from "eslint-plugin-html";
import globals from "globals";

export default [
  js.configs.recommended,
  {
    files: ["Jellyfin.Plugin.BetaSeries/Configuration/**/*.{js,html}"],
    plugins: { html },
    languageOptions: {
      sourceType: "script",
      // Globals provided by the Jellyfin web client.
      globals: { ...globals.browser, ApiClient: "readonly", Dashboard: "readonly" },
    },
  },
];
