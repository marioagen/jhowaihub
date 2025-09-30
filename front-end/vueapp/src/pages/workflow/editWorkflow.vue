<template>
    <WorkflowBoards 
        :isEdit="true"
        :id="routeId"
    />
</template>

<script>
    import WorkflowBoards from "@/components/workflow/WorkflowBoards.vue";
    export default {
        name: "EditWorkflow",
        components: {
            WorkflowBoards,
        },
        computed: {
            routeId() {
                return parseInt(this.$route.params.id);
            },
        },
    }
</script>