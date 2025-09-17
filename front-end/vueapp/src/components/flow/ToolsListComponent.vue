<template>
    <div>
        <button
            v-for="tool in toolsList"
            :key="tool.id"
            class="btn btn-outline-primary btn-sm me-2"
        >
            {{ tool.name }}
        </button>
    </div>
</template>

<script>
    export default {
        name: "ToolsList",
        data: () => ({
            toolsList: [
                { id: 0, name: "OCR" },
                { id: 1, name: "OCR II" },
                { id: 2, name: "OCR III" },
                { id: 3, name: "OCR IV" },
                { id: 4, name: "OCR V" },
            ],
        }),
    }
</script>
