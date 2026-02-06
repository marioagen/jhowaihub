<template>
    <main>
        <PromptForm :id="idEdit" @saved="redirectToPromptList" @cancelled="redirectToPromptList" />
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
        }
    },
    methods: {
        redirectToPromptList() {
            this.$router.push({ name: "Prompt" });
        },
    },
    mounted() {
        const queryId = this.$route.query.id;
        if (queryId) {
            this.idEdit = parseInt(queryId);
        }
    },
}
</script>
