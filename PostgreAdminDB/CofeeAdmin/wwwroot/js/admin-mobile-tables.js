(() => {
    const enhanceTable = (table) => {
        if (!(table instanceof HTMLTableElement) || !table.closest("main")) return;

        table.classList.add("admin-mobile-table");

        const headers = Array.from(table.querySelectorAll("thead th"), (header) =>
            header.textContent.trim().replace(/\s+/g, " ")
        );

        if (headers.length === 0) return;

        const lastHeader = headers[headers.length - 1].toLocaleLowerCase("tr-TR");
        if (lastHeader.includes("işlem")) {
            table.classList.add("has-mobile-actions");
        }

        table.querySelectorAll("tbody tr").forEach((row) => {
            const cells = Array.from(row.cells);
            if (cells.some((cell) => cell.colSpan > 1)) {
                row.classList.add("admin-table-empty-row");
                return;
            }

            cells.forEach((cell, index) => {
                if (!cell.hasAttribute("data-mobile-label")) {
                    cell.dataset.mobileLabel = headers[index] || `Bilgi ${index + 1}`;
                }
            });
        });
    };

    const enhanceTables = (root = document) => {
        if (root instanceof HTMLTableElement) enhanceTable(root);
        const containingTable = root.closest?.("table");
        if (containingTable) enhanceTable(containingTable);
        root.querySelectorAll?.("main table").forEach(enhanceTable);
    };

    enhanceTables();

    const observer = new MutationObserver((mutations) => {
        for (const mutation of mutations) {
            mutation.addedNodes.forEach((node) => {
                if (node.nodeType === Node.ELEMENT_NODE) enhanceTables(node);
            });
        }
    });

    if (document.body) {
        observer.observe(document.body, { childList: true, subtree: true });
    }
})();
