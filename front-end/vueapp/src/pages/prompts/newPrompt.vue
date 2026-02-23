<template>
    <main>
        <PromptForm
            :id="idEdit"
            :cloneId="idClone"
            @saved="redirectToPromptList"
            @cancelled="redirectToPromptList"
        />
    </main>
</template>
<script>
    import PromptForm from "@/components/prompts/PromptForm.vue";

    export default {
        name: "PromptComponent",
        components: {
            PromptForm,
        },
        data() {
            return {
                idEdit: null,
                idClone: null,
            };
        },
        methods: {
            redirectToPromptList() {
                this.$router.push({ name: "Prompt" });
            },
        },
        mounted() {
            const queryId = this.$route.query.id;
            const queryClone = this.$route.query.clone;
            if (queryId) {
                this.idEdit = parseInt(queryId);
            }
            if (queryClone) {
                this.idClone = parseInt(queryClone);
            }
        },
    };
</script>
