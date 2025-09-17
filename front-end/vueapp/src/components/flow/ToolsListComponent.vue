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
    import ToolsServices from '@/services/tools/ToolsServices';
    export default {
        name: "ToolsList",
        data: () => ({
            toolsList: [],
        }),
        methods: {
            getToolsList() {
                ToolsServices.getToolsList()
                    .then((response) => {
                        this.toolsList = response;
                    });
            },
        },
        created() {
            this.getToolsList();
        },
    }
</script>
